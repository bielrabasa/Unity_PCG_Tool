using UnityEngine;
using UnityEditor;

namespace PCG_Tool
{

    [CustomEditor(typeof(SBO_TileSet))]
    public class EDT_TileSet : Editor
    {
        public override void OnInspectorGUI()
        {
            SBO_TileSet tileSet = (SBO_TileSet)target;

            //Tile Size
            tileSet.tileSize = EditorGUILayout.Vector3Field("Tile Size", tileSet.tileSize);

            EditorGUILayout.LabelField("Tiles", EditorStyles.boldLabel);

            for (int i = 1; i < tileSet.tiles.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Id: " + i, GUILayout.Width(50));
                tileSet.tiles[i] = (GameObject)EditorGUILayout.ObjectField(tileSet.tiles[i], typeof(GameObject), false);
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    tileSet.tiles.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Tile"))
                tileSet.tiles.Add(null);

            if (GUI.changed)
                EditorUtility.SetDirty(tileSet);
        }
    }

}


