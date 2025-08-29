using UnityEditor;
using UnityEngine;

namespace PCG_Tool
{

    [CustomEditor(typeof(SCR_WFC_Solver))]
    public class EDT_WFC_Solver : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SCR_WFC_Solver solver = (SCR_WFC_Solver)target;

            GUI.backgroundColor = STY_Style.Activated_Color;
            if (GUILayout.Button("Generate", STY_Style.Button_Layout))
            {
                solver.Generate();
            }

            if (solver.debugMode)
            {
                if (GUILayout.Button("Step", STY_Style.Button_Layout))
                {
                    solver.StepDebugSolver();
                }
            }

            if (solver.debugMode)
            {
                if (GUILayout.Button("Debug Timed Solve", STY_Style.Button_Layout))
                {
                    solver.TimedDebugSolver();
                }
            }
        }
    }

}
