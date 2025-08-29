using UnityEditor;
using UnityEngine;
using PCG_Tool;

[CustomEditor(typeof(SCR_ModelPrinter))]
public class EDT_ModelPrinter : Editor
{
    public override void OnInspectorGUI()
    {
        SCR_ModelPrinter printer = (SCR_ModelPrinter)target;

        //Inspector design
        EditorGUILayout.LabelField("Model Printer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        //Inspector
        printer.tileSet = (SBO_TileSet)EditorGUILayout.ObjectField("Tile Set", printer.tileSet, typeof(SBO_TileSet), false);
        printer.model = (SBO_RepresentationModel)EditorGUILayout.ObjectField("Representation Model", printer.model, typeof(SBO_RepresentationModel), false);
        printer.parentTransform = (Transform)EditorGUILayout.ObjectField("Parent Transform", printer.parentTransform, typeof(Transform), true);

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Generation", EditorStyles.whiteLabel);
        printer.generateOnStart = EditorGUILayout.Toggle("Generate On Start", printer.generateOnStart);
        printer.updateChanges = EditorGUILayout.Toggle("Update Changes", printer.updateChanges);

        EditorGUILayout.Space(10);

        if (GUI.changed)
        {
            printer.OnValidate();
        }

        //Generate button
        GUI.backgroundColor = STY_Style.Positive_Color;
        if (GUILayout.Button("Print Model", STY_Style.Button_Layout))
        {
            printer.Generate();
        }

        EditorGUILayout.Space(5);

        //Erase button
        GUI.backgroundColor = STY_Style.Negative_Color;
        if (GUILayout.Button("Clear", STY_Style.Button_Layout))
        {
            printer.ClearTiles();
        }
    }
}
