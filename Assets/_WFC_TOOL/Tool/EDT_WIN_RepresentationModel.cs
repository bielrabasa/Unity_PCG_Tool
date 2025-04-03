using UnityEditor;
using UnityEngine;

namespace PCG_Tool
{

    public class EDT_WIN_RepresentationModel : EditorWindow
    {
        private SBO_RepresentationModel model;
        private short selectedTileId = 1;
        private TileOrientation tileOrientation = TileOrientation.None;
        private int currentLayer = 0; // Editing grid layer


        public static void ShowWindow(SBO_RepresentationModel targetModel)
        {
            var window = GetWindow<EDT_WIN_RepresentationModel>("Model Editor");
            window.model = targetModel;
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Representation Model Editor", EditorStyles.boldLabel);
            //model = (SBO_RepresentationModel)EditorGUILayout.ObjectField("Representation Model", model, typeof(SBO_RepresentationModel), false);

            if (model == null) return;
            if (model.tileSet == null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Representation Model does not have a TileSet!", EditorStyles.whiteLargeLabel);
                return;
            }

            // Layer selector
            currentLayer = EditorGUILayout.IntSlider("Layer (Y)", currentLayer, 0, model.GridSize.y - 1);

            EditorGUILayout.Space();

            // Grid of buttons in editor
            for (int z = model.GridSize.z - 1; z >= 0; z--) // Z goes backwards to match view direction
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < model.GridSize.x; x++)
                {
                    int tileId = model.GetTile(x, currentLayer, z).id;
                    string buttonLabel = tileId > 0 ? tileId.ToString() : ".";

                    if (GUILayout.Button(buttonLabel, GUILayout.Width(30), GUILayout.Height(30)))
                    {
                        Undo.RecordObject(model, "Change Tile");
                        model.SetTile(x, currentLayer, z, new TileInfo(selectedTileId, tileOrientation));
                        
                        EditorUtility.SetDirty(model); // Modify object
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();

            // Selección del Tile
            EditorGUILayout.LabelField("Select a Tile:");
            selectedTileId = (short)EditorGUILayout.IntSlider(selectedTileId, 0, model.tileSet.GetTileCount() - 1);

            EditorGUILayout.LabelField("Select tile options:");
            tileOrientation = (TileOrientation)EditorGUILayout.EnumFlagsField("Orientation", tileOrientation);

            if (GUILayout.Button("Clear Layer"))
            {
                Undo.RecordObject(model, "Clear Layer");
                for (int x = 0; x < model.GridSize.x; x++)
                    for (int z = 0; z < model.GridSize.z; z++)
                        model.SetTile(x, currentLayer, z, new TileInfo(0));

                EditorUtility.SetDirty(model);
            }
        }
    }

}