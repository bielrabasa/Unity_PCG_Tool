using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace PCG_Tool
{

    [ExecuteInEditMode]
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
        private List<TileVariant> _variants;

        //Output
        public SBO_RepresentationModel outputModel;

        ///
        /// METHODS
        ///

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

                if (initialRepresentationModel.GridSize != gridSize)
                {
                    Debug.LogWarning("Initial Representation Model GridSize must be the same as WFC_Solver GridSize.");
                    return false;
                }
            }

            _tileSet = rules.tileSet;
            return true;
        }

        public void Generate()
        {
            if (!CheckInformation()) return;

            SetupGridCells(); //TODO: Optimisation, only call if anything changed, not always
            CallSolver();

            FillRepresentationModel();
            PrintModel(); //TODO: Erase test
        }

        void SetupGridCells()
        {
            //TODO: Setup variants Call on generate, optimise to only call when anything changes?
            //TODO: if initialRepresentationModel != null modify generated tiles (DO IT HERE OR IN ALGORITHM)
            _gridCells = new GridCell[gridSize.x, gridSize.y, gridSize.z];

            _variants = CalculateAllVariants();

            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    for (int k = 0; k < gridSize.z; k++)
                    {
                        //Creating all the cells and duplicating the AllVariants list so that all the cells do NOT share the same INSTANCE for the list. 
                        _gridCells[i, j, k] = new GridCell(new Vector3Int(i, j, k), new List<TileVariant>(_variants), rules);
                    }
                }
            }
        }

        List<TileVariant> CalculateAllVariants()
        {
            List<TileVariant> variants = new List<TileVariant>();

            for (int i = 0; i < rules.tileRules.Length; i++)
            {
                if (rules.tileRules[i].weight == 0f) continue;

                variants = variants.Concat(TileVariant.GenerateVariantsFromTileRule(rules.tileRules[i], (short)i)).ToList();
            }

            //Debug.Log("Variant count: " +  variants.Count + " Rule count: " + rules.tileRules.Length); 

            return variants;
        }

        void CallSolver()
        {
            _solver = new WFC_Algorithm(_gridCells, gridSize, rules, _variants);
            if(initialRepresentationModel != null) _solver._initialRepresentationModel = initialRepresentationModel;

            if (debugMode) _solver.GenerateDebugMode();
            else _solver.GenerateStandard();
        }

        public void StepDebugSolver()
        {
            if (!debugMode) return;

            _solver.StepDebugMode();

            FillRepresentationModel();
            PrintModel();
        }

        //Timed Debug solve
        public void TimedDebugSolver()
        {
            if(!debugMode) return;

            Generate();
            EditorApplication.update += TimerDebugSolve;
        }

        private void TimerDebugSolve()
        {
            StepDebugSolver();
            if (_solver.finished) EditorApplication.update -= TimerDebugSolve;
        }

        //OUTPUT
        void FillRepresentationModel()
        {
            if (outputModel == null)
            {
                outputModel = ScriptableObject.CreateInstance<SBO_RepresentationModel>();
                outputModel.tileSet = _tileSet;
            }
            outputModel.GridSize = gridSize;

            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    for (int k = 0; k < gridSize.z; k++)
                    {
                        GridCell cell = _gridCells[i, j, k];
                        outputModel.SetTile(cell.coords.x, cell.coords.y, cell.coords.z, (cell.chosenVariant == null)? new TileInfo(-1, TileOrientation.None) : cell.chosenVariant.tileInfo);
                    }
                }
            }
        }

        //TEST
        void PrintModel()
        {
            SCR_ModelPrinter printer = gameObject.GetComponent<SCR_ModelPrinter>();

            if (printer == null)
            {
                Debug.Log("ERROR: Trying to print a model, no printer attached to the gameObject");
                return;
            }

            printer.model = outputModel;
            printer.tileSet = _tileSet;

            printer.PrintModel();
        }
    }

}
