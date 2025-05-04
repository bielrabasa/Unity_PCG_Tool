using PCG_Tool;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SCR_WFC_Solver : MonoBehaviour
{
    //General info
    private SBO_TileSet _tileSet;
    public SBO_Rules rules;
    public SBO_RepresentationModel initialRepresentationModel;
    public Vector3Int gridSize = Vector3Int.one;


    //Generation methods
    public bool generateOnStart = true;
    public bool debugMode = false;

    //Variables
    private GridCell[,,] _gridCells;
    private WFC_Algorithm _solver;

    void Start()
    {
        if (generateOnStart) Generate();
    }

    bool CheckInformation()
    {
        if (rules == null)
        {
            Debug.LogWarning("Rules must be assigned to WFC_Solver.");
            return false;
        }

        if (rules.tileSet == null)
        {
            Debug.LogWarning("No tileSet assigned to the rules.");
            return false;
        }

        if (initialRepresentationModel != null)
        {
            if (initialRepresentationModel.tileSet != rules.tileSet)
            {
                Debug.LogWarning("Initial Representation Model & Rules must have the same tileSet assigned.");
                return false;
            }

            if(initialRepresentationModel.GridSize != gridSize)
            {
                Debug.LogWarning("Initial Representation Model GridSize must be the same as WFC_Solver GridSize.");
                return false;
            }
        }

        _tileSet = rules.tileSet;
        return true;
    }

    void Generate()
    {
        if (!CheckInformation()) return;

        SetupGridCells(); //TODO: Optimisation, only call if anything changed, not always
        CallSolver();
    }

    void SetupGridCells()
    {
        //TODO: Call on generate, optimise to only call when anything changes?
        //TODO: if initialRepresentationModel != null modify generated tiles
        _gridCells = new GridCell[gridSize.x, gridSize.y, gridSize.z];

        var AllVariants = CalculateAllVariants();

        for(int i = 0; i < gridSize.x; i++)
        {
            for(int j = 0; j < gridSize.y; j++)
            {
                for (int k = 0; k < gridSize.z; k++)
                {
                    //Creating all the cells and duplicating the AllVariants list so that all the cells do NOT share the same INSTANCE for the list. 
                    _gridCells[i, j, k] = new GridCell(new Vector3Int(i, j, k), new List<TileVariant>(AllVariants));
                }
            }
        }
    }

    List<TileVariant> CalculateAllVariants()
    {
        List<TileVariant> variants = new List<TileVariant>();

        for(int i = 0; i < rules.tileRules.Length; i++)
        {
            variants = variants.Concat(TileVariant.GenerateVariantsFromTileRule(rules.tileRules[i], (short)i)).ToList();
        }

        return variants;
    }

    void CallSolver()
    {
        _solver = new WFC_Algorithm(_gridCells);

        if (debugMode) _solver.GenerateDebugMode();
        else _solver.GenerateStandard();
    }

    void StepDebugSolver()
    {
        if (!debugMode) return;

        _solver.StepDebugMode();
    }
}
