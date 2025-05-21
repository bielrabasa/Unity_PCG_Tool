using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;

namespace PCG_Tool
{

    public class WFC_Algorithm
    {
        public bool finished {  get; private set; }

        private GridCell[,,] _gridCells;
        private List<GridCell> _cellsByEntropy; //TODO: implement
        private Vector3Int _gridSize;
        private SBO_Rules _rules;
        
        //Debug
        private bool _debugMode;
        private Vector3Int currentCollapsedTile; //TODO: erase, not doing by entropy

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
            finished = false;

            //TODO
            Loop();
        }

        public void GenerateDebugMode()
        {
            _debugMode = true;
            finished = false;
            currentCollapsedTile = new Vector3Int(0, 0, -1);

            //TODO
        }

        public void StepDebugMode()
        {
            if(finished) return;

            if (!_debugMode)
            {
                Debug.Log("Trying to Step a non-debug WFC algorithm.");
                return;
            }

            //TODO
            Vector3Int tileToStep = currentCollapsedTile + Vector3Int.forward;
            if(tileToStep.z >= _gridSize.z)
            {
                tileToStep.z = 0;
                tileToStep.y++;

                if (tileToStep.y >= _gridSize.y)
                {
                    tileToStep.y = 0;
                    tileToStep.x++;

                    if (tileToStep.x >= _gridSize.x)
                    {
                        finished = true;
                        return;
                    }
                }
            }

            _gridCells[tileToStep.x, tileToStep.y, tileToStep.z].CollapseCell();
            Propagate(tileToStep);
            currentCollapsedTile = tileToStep;
        }

        //-------------------------------------------------------
        //                      WFC STEPS
        //-------------------------------------------------------

        void Loop()
        {
            //TODO: Algorithm loop
            for (int i = 0; i < _gridSize.x; i++)
            {
                for (int j = 0; j < _gridSize.y; j++)
                {
                    for (int k = 0; k < _gridSize.z; k++)
                    {
                        _gridCells[i, j, k].CollapseCell();
                        Propagate(new Vector3Int(i, j, k));
                    }
                }
            }

            finished = true;
        }

        void CollapseLeastEntropy()
        {
            //TODO
        }

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
    }

}