using PCG_Tool;
using UnityEngine;

public class WFC_Algorithm
{
    private GridCell[,,] _gridCells;
    private Vector3Int _gridSize;
    private bool _debugMode;

    public WFC_Algorithm(GridCell[,,] gridCells, Vector3Int gridSize) 
    { 
        this._gridCells = gridCells;
        this._gridSize = gridSize;
    }

    public void GenerateStandard()
    {
        _debugMode = false;

        //TODO
        Loop();
    }

    public void GenerateDebugMode()
    {
        _debugMode = true;

        //TODO
    }

    public void StepDebugMode()
    {
        if (!_debugMode)
        {
            Debug.Log("Trying to Step a non-debug WFC algorithm.");
            return;
        }

        //TODO
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
                }
            }
        }
    }

    void CollapseLeastEntropy()
    {

    }

    void Propagate(Vector3Int tileToPropagateAround)
    {

    }
}
