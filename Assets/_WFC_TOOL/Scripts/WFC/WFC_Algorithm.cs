using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
        
        //Debug
        private bool _debugMode;
        //private Vector3Int currentCollapsedTile; //TODO: erase, not doing by entropy

        private static readonly Vector3Int[] NEIGHBOUR_DIRECTIONS = new Vector3Int[] { 
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right, Vector3Int.forward, Vector3Int.back };

        public WFC_Algorithm(GridCell[,,] gridCells, Vector3Int gridSize, SBO_Rules rules)
        {
            this._gridCells = gridCells;
            this._gridSize = gridSize;
            this._rules = rules;
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
            _cellsByEntropy.Clear();

            if (_initialRepresentationModel != null) ReduceAroundRM();
            if(_rules.airBorders) ReduceInitialBorders();

            //Setup entropy list
            for (int i = 0; i < _gridSize.x; i++)
                for (int j = 0; j < _gridSize.y; j++)
                    for (int k = 0; k < _gridSize.z; k++)
                    {
                        GridCell cell = _gridCells[i, j, k];
                        _cellsByEntropy.Add(cell);
                    }

            SortCells();
        }

        void Loop()
        {
            while (_cellsByEntropy.Count > 0)
            {
                StepAlgorithm();
            }

            finished = true;
        }

        void StepAlgorithm()
        {
            GridCell cell = GetTileToCollapse();

            cell.CollapseCell();
            _cellsByEntropy.Remove(cell);

            Propagate(cell.coords);
            SortCells();
        }

        //Entropy
        GridCell GetTileToCollapse()
        {
            //TODO: Random between least entropy
            return _cellsByEntropy[0];
        }

        //Compatibility
        void Propagate(Vector3Int tileToPropagateAround)
        {
            //Check the just-collapsed tile neighbours and reduce their compatibilities
            //TODO: this can be done in different threads in the future
            foreach (Vector3Int dir in NEIGHBOUR_DIRECTIONS) ReduceNonCompatible(tileToPropagateAround + dir);
        }

        void ReduceNonCompatible(Vector3Int tile)
        {
            //Check if this tile is valid
            if (!IsCoordInGrid(tile)) return;
            if (_gridCells[tile.x, tile.y, tile.z].collapsed) return;

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
            //TODO
            /*for (int i = 0; i < _gridSize.x; i++)
                for (int j = 0; j < _gridSize.y; j++)
                    for (int k = 0; k < _gridSize.z; k++)
                    {
                        TileInfo tile = _initialRepresentationModel.GetTile(i, j, k);


                    }*/
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