using UnityEditor;
using UnityEngine;

namespace PCG_Tool
{

    [CustomEditor(typeof(SBO_RepresentationModel))]
    public class EDT_RepresentationModel : Editor
    {
        public override void OnInspectorGUI()
        {
            SBO_RepresentationModel model = (SBO_RepresentationModel)target;

            //Inspector design
            EditorGUILayout.LabelField("Representation Model", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            //Inspector
            model.tileSet = (SBO_TileSet)EditorGUILayout.ObjectField("Tile Set", model.tileSet, typeof(SBO_TileSet), false);
            Vector3Int newGridSize = EditorGUILayout.Vector3IntField("Grid Size", model.GridSize);

            if(newGridSize.x <= 0) newGridSize.x = 1;
            if(newGridSize.y <= 0) newGridSize.y = 1;
            if(newGridSize.z <= 0) newGridSize.z = 1;

            model.GridSize = newGridSize;

            EditorGUILayout.Space(10);
            
            //Generate button
            GUI.backgroundColor = STY_Style.Deactivated_Color;
            if (GUILayout.Button("Manual Edit", STY_Style.Button_Layout))
            {
                EDT_WIN_RepresentationModel.ShowWindow(model);
            }
        }
    }

}
