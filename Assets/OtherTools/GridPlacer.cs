using UnityEngine;
using UnityEditor;

public class GridPlacer : EditorWindow
{
    private int columns = 5;
    private float spacing = 2.0f;

    [MenuItem("Tools/Grid Placer")]
    public static void ShowWindow()
    {
        GetWindow(typeof(GridPlacer));
    }

    void OnGUI()
    {
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);
        columns = EditorGUILayout.IntField("Columns", columns);
        spacing = EditorGUILayout.FloatField("Spacing", spacing);

        if (GUILayout.Button("Arrange in Grid"))
        {
            ArrangeObjects();
        }
    }

    void ArrangeObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected!");
            return;
        }

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            int row = i / columns;
            int col = i % columns;
            Vector3 newPosition = new Vector3(col * spacing, 0, row * spacing);
            selectedObjects[i].transform.position = newPosition;
        }
    }
}

