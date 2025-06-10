using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;

namespace PCG_Tool
{

    public class WFC_Algorithm
    {
        public bool finished {  get; private set; }

        //Cells
        private GridCell[,,] _gridCells;
        private List<GridCell> _cellsByEntropy = new List<GridCell>();

        //Settings
        private Vector3Int _gridSize;
        private SBO_Rules _rules;
        public SBO_RepresentationModel _initialRepresentationModel;

        //Backtracking
        private bool _allowBacktracking = true;
        private List<TileVariant> _allVariants;
        private List<GridCell> _backtrackingOrder;
        private Vector3Int lastCollapsedPos = Vector3Int.back;

        //Debug
        private bool _debugMode;

        private static readonly Vector3Int[] NEIGHBOUR_DIRECTIONS = new Vector3Int[] { 
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right, Vector3Int.forward, Vector3Int.back };

        public WFC_Algorithm(GridCell[,,] gridCells, Vector3Int gridSize, SBO_Rules rules, List<TileVariant> allVariants)
        {
            this._gridCells = gridCells;
            this._gridSize = gridSize;
            this._rules = rules;
            this._allVariants = allVariants;
        }

        public void GenerateStandard()
        {
            _debugMode = false;

            PrepareGeneration();
            Loop();
        }

        public void GenerateDebugMode()
        {
            _debugMode = true;

            PrepareGeneration();
        }

        public void StepDebugMode()
        {
            if(finished) return;

            if (!_debugMode)
            {
                Debug.Log("Trying to Step a non-debug WFC algorithm.");
                return;
            }

            StepAlgorithm();

            if(_cellsByEntropy.Count == 0) finished = true;
        }

        //-------------------------------------------------------
        //                      WFC STEPS
        //-------------------------------------------------------

        void PrepareGeneration()
        {
            finished = false;

            //Setup entropy list
            _cellsByEntropy.Clear();

            for (int i = 0; i < _gridSize.x; i++)
                for (int j = 0; j < _gridSize.y; j++)
                    for (int k = 0; k < _gridSize.z; k++)
                    {
                        GridCell cell = _gridCells[i, j, k];
                        _cellsByEntropy.Add(cell);
                    }

            //Reduce initial options
            if (_initialRepresentationModel != null) ReduceAroundRM();
            else if(_rules.airBorders) ReduceInitialBorders();

            SortCells();
        }

        void Loop()
        {
            while (!finished && _cellsByEntropy.Count > 0)
            {
                StepAlgorithm();
            }

            finished = true;
        }

        void StepAlgorithm()
        {
            if (finished) return;

            GridCell cell = GetTileToCollapse();

            cell.CollapseCell();
            _cellsByEntropy.Remove(cell);

            if (!_allowBacktracking) Propagate(cell.coords);
            else if (!Propagate(cell.coords))
            {
                PropagateNotifyCollapsedFrom(cell.coords, add: true);
                SortCells();

                //Backtrack on the cell that has no possibilities
                Backtrack(_cellsByEntropy[0]);
            }
            else
            {
                //Correct Propagation
                PropagateNotifyCollapsedFrom(cell.coords, add: true);
            }

            SortCells();
        }

        //Entropy
        GridCell GetTileToCollapse()
        {
            //TODO: TESTING
            /*Vector3Int lastCollapsedCell = lastCollapsedPos;

            lastCollapsedPos.z += 1;

            if(lastCollapsedPos.z >= _gridSize.z)
            {
                lastCollapsedPos.z = 0;
                lastCollapsedPos.x++;
            }

            if (lastCollapsedPos.x >= _gridSize.x)
            {
                lastCollapsedPos.x = 0;
                lastCollapsedPos.y++;
            }

            if(lastCollapsedPos.y >= _gridSize.y)
            {
                finished = true;
                return null;
            }

            GridCell cell = _gridCells[lastCollapsedPos.x, lastCollapsedPos.y, lastCollapsedPos.z];

            if(IsCoordInGrid(lastCollapsedCell)) cell.collapsedFrom = _gridCells[lastCollapsedCell.x, lastCollapsedCell.y, lastCollapsedCell.z];
            return cell;*/


            //TODO: Random between least entropy
            return _cellsByEntropy[0];
        }

        /// <returns> Returns false if blocks the possibility of collapsing another tile (entropy 0) </returns>
        bool Propagate(Vector3Int tileToPropagateAround)
        {
            //Check the just-collapsed tile neighbours and reduce their compatibilities
            //TODO: this can be done in different threads in the future

            bool success = true;
            foreach (Vector3Int dir in NEIGHBOUR_DIRECTIONS)
            {
                if(!ReduceNonCompatible(tileToPropagateAround + dir)) success = false;
            }

            return success;
        }

        //Add -> True | Remove -> False
        void PropagateNotifyCollapsedFrom(Vector3Int tileToPropagateAround, bool add)
        {
            foreach (Vector3Int dir in NEIGHBOUR_DIRECTIONS)
            {
                Vector3Int coords = tileToPropagateAround + dir;
                if (!IsCoordInGrid(coords)) continue;

                GridCell cell = _gridCells[coords.x, coords.y, coords.z];

                if (add)
                {
                    cell.collapsedFromSorted.Add(_gridCells[tileToPropagateAround.x, tileToPropagateAround.y, tileToPropagateAround.z]);
                }
                else
                {
                    cell.collapsedFromSorted.Remove(_gridCells[tileToPropagateAround.x, tileToPropagateAround.y, tileToPropagateAround.z]);
                }
            }
        }

        void PropagateRefill(Vector3Int tileToPropagateAround)
        {
            foreach (Vector3Int dir in NEIGHBOUR_DIRECTIONS)
            {
                Vector3Int pos = tileToPropagateAround + dir;

                if(!IsCoordInGrid(pos)) continue;

                GridCell cell = _gridCells[pos.x, pos.y, pos.z];
                if (cell.collapsed) continue;

                cell.RefillVariants(_allVariants);
            }
        }

        /// <returns> Returns false if the tile ends with entropy 0 </returns>
        bool ReduceNonCompatible(Vector3Int tile, bool reduceIfCollapsed = false)
        {
            //Check if this tile is valid
            if (!IsCoordInGrid(tile)) return true;
            if (!reduceIfCollapsed && _gridCells[tile.x, tile.y, tile.z].collapsed) return true;

            //Check all neighbouring tiles to this one
            for (int i = 0; i < NEIGHBOUR_DIRECTIONS.Length; i++)
            {
                Vector3Int nbTile = tile + NEIGHBOUR_DIRECTIONS[i];

                if (IsValidTileToCheck(nbTile))
                {
                    TileVariant nbVariant = _gridCells[nbTile.x, nbTile.y, nbTile.z].chosenVariant;
                    if (nbVariant == null) continue;

                    _gridCells[tile.x, tile.y, tile.z].CheckAllVariantsToFace(nbVariant, (FaceDirection)i);
                }
                //Off the grid (border)
                else if (_rules.airBorders && !IsCoordInGrid(nbTile))
                {
                    _gridCells[tile.x, tile.y, tile.z].CheckAllVariantsToFace(TileVariant.AIR_VARIANT, (FaceDirection)i);
                }
            }

            if (_gridCells[tile.x, tile.y, tile.z].entropy == 0) return false;
            return true;
        }

        void ReduceInitialBorders()
        {
            for (int x = 0; x < _gridSize.x; x++)
                for (int y = 0; y < _gridSize.y; y++)
                    for (int z = 0; z < _gridSize.z; z++)
                    {
                        Vector3Int pos = new Vector3Int(x, y, z);
                        if (IsOnBorder(pos)) ReduceNonCompatible(pos);
                    }
        }

        void ReduceAroundRM()
        {
            for (int i = 0; i < _gridSize.x; i++)
                for (int j = 0; j < _gridSize.y; j++)
                    for (int k = 0; k < _gridSize.z; k++)
                    {
                        TileInfo tile = _initialRepresentationModel.GetTile(i, j, k);
                        if (tile.id == -1) continue;

                        //Setup collapsed cell
                        GridCell cell = _gridCells[i, j, k];
                        _cellsByEntropy.Remove(cell);
                        cell.CollapseManually(TileVariant.GetTileVariantFromTileInfo(tile, _rules.tileRules[tile.id]));
                    }

            foreach(GridCell cell in _cellsByEntropy)
            {
                ReduceNonCompatible(cell.coords);
            }
        }

        void Backtrack(GridCell cellBacktrackingAround)
        {
            _backtrackingOrder = cellBacktrackingAround.collapsedFromSorted;
            int currentBacktrackingOrder = _backtrackingOrder.Count - 1;
            GridCell cellToBacktrack = _backtrackingOrder[currentBacktrackingOrder];

            while (cellToBacktrack != null)
            {
                if(cellToBacktrack.entropy == 0)
                {
                    Debug.LogError("ERROR: Caught in a backtracking loop.");
                    finished = true;
                    return;
                }

                //Forbid chosen variant
                ReduceNonCompatible(cellToBacktrack.coords, reduceIfCollapsed: true);
                bool succesfulCollapse = cellToBacktrack.ReCollapseCell();

                //Re-fill adjacent tiles
                PropagateRefill(cellToBacktrack.coords);

                //Re-try with the next variant
                if (succesfulCollapse)
                {
                    //TODO PropagateNotifyCollapsedFrom(cellToBacktrack.coords, add: false);

                    //Try to propagate
                    if (Propagate(cellToBacktrack.coords))
                    {
                        //TODO: TESTING -> when succesful collapse & propagation -> reset position
                        //PropagateNotifyCollapsedFrom(cellToBacktrack.coords, add: true); //NO PROPAGATION, surrounding tiles already notified
                        cellToBacktrack = null;
                    }
                }
                
                //If current cell has no more options
                else
                {
                    //Reduce neighbour options (because they got refilled)
                    if (!Propagate(cellToBacktrack.coords))
                    {
                        Debug.LogError("ERROR: Neighbouring tiles should not have entropy 0 in this case.");
                    }

                    //Resetting tile state to previous
                    PropagateNotifyCollapsedFrom(cellToBacktrack.coords, add: false);
                    cellToBacktrack.RefillVariants(_allVariants);
                    ReduceNonCompatible(cellToBacktrack.coords);
                    _cellsByEntropy.Add(cellToBacktrack);

                    //Next tile
                    currentBacktrackingOrder--;
                    if(currentBacktrackingOrder < 0)
                    {
                        Backtrack(cellToBacktrack);
                    }
                    else
                    {
                        cellToBacktrack = _backtrackingOrder[currentBacktrackingOrder];
                    }
                }
            }
        }

        //-------------------------------------------------------
        //                      HELPERS
        //-------------------------------------------------------

        //Tile exists in grid & isCollapsed
        private bool IsValidTileToCheck(Vector3Int coord)
        {
            return IsCoordInGrid(coord) && _gridCells[coord.x, coord.y, coord.z].collapsed;
        }

        private bool IsCoordInGrid(Vector3Int coord)
        {
            return (coord.x >= 0 && coord.x < _gridSize.x) && 
                (coord.y >= 0 && coord.y < _gridSize.y) && 
                (coord.z >= 0 && coord.z < _gridSize.z);
        }

        private bool IsOnBorder(Vector3Int coord)
        {
            return coord.x == 0 || coord.x == _gridSize.x - 1 ||
                    coord.y == 0 || coord.y == _gridSize.y - 1 ||
                    coord.z == 0 || coord.z == _gridSize.z - 1;
        }

        //Sorting
        void SortCells()
        {
            _cellsByEntropy.Sort((a, b) => a.entropy.CompareTo(b.entropy));
        }
    }

}