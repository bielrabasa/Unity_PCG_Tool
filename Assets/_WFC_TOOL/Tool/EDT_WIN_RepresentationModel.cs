using UnityEditor;
using UnityEngine;

namespace PCG_Tool
{

    public class EDT_WIN_RepresentationModel : EditorWindow
    {
        private SBO_RepresentationModel model;
        private SBO_TileSet tileSet;
        private short selectedTileId = 1;
        private int currentLayer = 0; // Editing grid layer

        [MenuItem("Tools/Representation Model Editor")]
        public static void ShowWindow()
        {
            GetWindow<EDT_WIN_RepresentationModel>("Model Editor");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Representation Model Editor", EditorStyles.boldLabel);
            model = (SBO_RepresentationModel)EditorGUILayout.ObjectField("Representation Model", model, typeof(SBO_RepresentationModel), false);
            tileSet = (SBO_TileSet)EditorGUILayout.ObjectField("Tile Set", tileSet, typeof(SBO_TileSet), false);

            if (model == null || tileSet == null) return;

            // Selector de Capa
            currentLayer = EditorGUILayout.IntSlider("Capa (Y)", currentLayer, 0, model.gridSize.y - 1);

            EditorGUILayout.Space();

            // Dibujar la grid en el Editor
            for (int z = 0; z < model.gridSize.z; z++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < model.gridSize.x; x++)
                {
                    int tileId = model.GetTile(x, currentLayer, z).id;
                    string buttonLabel = tileId > 0 ? tileId.ToString() : ".";

                    if (GUILayout.Button(buttonLabel, GUILayout.Width(30), GUILayout.Height(30)))
                    {
                        Undo.RecordObject(model, "Change Tile");
                        model.SetTile(x, currentLayer, z, new TileInfo(selectedTileId));
                        EditorUtility.SetDirty(model); // Marca el objeto como modificado
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            // Selección del Tile
            EditorGUILayout.LabelField("Selecciona un Tile:");
            selectedTileId = (short)EditorGUILayout.IntSlider(selectedTileId, 1, tileSet.GetTileCount() - 1);

            if (GUILayout.Button("Limpiar Capa"))
            {
                Undo.RecordObject(model, "Clear Layer");
                for (int x = 0; x < model.gridSize.x; x++)
                    for (int z = 0; z < model.gridSize.z; z++)
                        model.SetTile(x, currentLayer, z, new TileInfo(0));

                EditorUtility.SetDirty(model);
            }
        }
    }

}