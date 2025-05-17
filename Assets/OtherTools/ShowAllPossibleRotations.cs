using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using PCG_Tool;

public class ShowAllPossibleRotations : EditorWindow
{
    bool allowXRot = false;
    bool allowYRot = false;
    bool allowZRot = false;
    bool allowMirror = false;
    float distance = 2f;

    [MenuItem("Tools/ShowAllPossibleRotations")]
    public static void ShowWindow()
    {
        GetWindow(typeof(ShowAllPossibleRotations));
    }

    void OnGUI()
    {
        GUILayout.Label("Show Rotation Settings", EditorStyles.boldLabel);

        allowXRot = EditorGUILayout.Toggle("Allow X Rotation", allowXRot);
        allowYRot = EditorGUILayout.Toggle("Allow Y Rotation", allowYRot);
        allowZRot = EditorGUILayout.Toggle("Allow Z Rotation", allowZRot);
        allowMirror = EditorGUILayout.Toggle("Allow Mirroring", allowMirror);

        if (GUILayout.Button("Show"))
        {
            ArrangeObjects();
        }
    }

    void ArrangeObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length != 1)
        {
            Debug.LogWarning("Select only 1 object");
            return;
        }

        Transform parent = new GameObject("Rotation Combinations").transform;

        Vector3 pos = Vector3.zero;

        TileRule rule = new TileRule();
        rule.constraints =
            (allowXRot ? TileConstraints.Allow_X_Rotation : TileConstraints.None) |
            (allowYRot ? TileConstraints.Allow_Y_Rotation : TileConstraints.None) |
            (allowZRot ? TileConstraints.Allow_Z_Rotation : TileConstraints.None);

        List<Quaternion> rotations = rule.GetPossibleRotations();

        Debug.Log("Possible Rotations: " + rotations.Count);

        foreach (Quaternion rot in rotations)
        {
            //Calculate position
            Instantiate(selectedObjects[0], pos, rot, parent);

            //Mirrors
            if (allowMirror)
            {
                Instantiate(selectedObjects[0], pos + Vector3.forward * distance, rot, parent).transform.localScale = new Vector3(-1, 1, 1);
            }

            pos.x += distance;
        }
    }
}

