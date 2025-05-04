using PCG_Tool;
using UnityEngine;

public class WFC_Algorithm
{
    private GridCell[,,] _gridCells;
    private bool _debugMode;

    public WFC_Algorithm(GridCell[,,] gridCells) 
    { 
        this._gridCells = gridCells;
    }

    public void GenerateStandard()
    {
        _debugMode = false;

        //TODO
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

    void CollapseLeastEntropy()
    {

    }

    void Propagate(Vector3Int tileToPropagateAround)
    {

    }
}
