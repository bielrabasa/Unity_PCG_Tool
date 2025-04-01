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

        //Generate button
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Generate", GUILayout.Height(30)))
        {
            printer.Generate();
        }

        EditorGUILayout.Space(5);

        //Erase button
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Clear", GUILayout.Height(30)))
        {
            printer.ClearTiles();
        }
    }
}
