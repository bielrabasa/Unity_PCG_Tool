using UnityEngine;
using UnityEditor;

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

        //Rotation X
        Vector3 pos = Vector3.zero;
        for (int i = 0; i < 4; i++) {

            Quaternion rot = Quaternion.Euler(i * 90, 0, 0);

            //Calculate position
            Instantiate(selectedObjects[0], pos, rot, parent);

            //Mirrors
            if (allowMirror)
            {
                Instantiate(selectedObjects[0], pos + Vector3.forward * distance, rot, parent).transform.localScale = new Vector3(-1, 1, 1);
            }

            pos.x += distance;
        }

        return;


        Vector3[] ups = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        Vector3[] forwards = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        //TODO: Solve for only X rotation
        //Y rot -> ups = Vector.Up
        //Z rot -> forwards = Vector.Forward

        //Vector3 pos = Vector3.zero;
        foreach (Vector3 upDir in ups)
        {
            foreach (Vector3 forwardDir in forwards)
            {
                //Ignore non-orthogonal directions
                float dot = Vector3.Dot(upDir, forwardDir);
                if (dot > 0.1f || dot < -0.1f) continue;

                //Calculate complete rotation
                Quaternion rot = Quaternion.LookRotation(forwardDir, upDir);

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
}

