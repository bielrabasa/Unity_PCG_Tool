using System.IO;
using UnityEditor;
using UnityEngine;

namespace PCG_Tool
{

    public class EDT_WIN_AutoSetup : EditorWindow
    {
        [SerializeField] GameObject prefabToInstanciate;
        private string folderName = "newWFCFolder";
        private bool setScenePrefab = true;
        private SBO_Rules sbo_rules;

        [MenuItem("WFC/AutoSetup")]
        public static void ShowWindow()
        {
            GetWindow<EDT_WIN_AutoSetup>("WFC AutoSetup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Set a name for this model setup:", EditorStyles.boldLabel);
            folderName = EditorGUILayout.TextField(folderName);

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Set a prefab in the current scene:", EditorStyles.boldLabel);
            setScenePrefab = EditorGUILayout.Toggle(setScenePrefab);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Create"))
            {
                if (ProcessSelectedPrefabs())
                {
                    if (setScenePrefab) SetScenePrefab();
                }
                Close();
            }
        }

        private bool ProcessSelectedPrefabs()
        {
            if (string.IsNullOrEmpty(folderName))
            {
                EditorUtility.DisplayDialog("Error", "A name must be set.", "OK");
                return false;
            }

            //Test that all assets are .prefabs
            foreach (Object obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab"))
                {
                    EditorUtility.DisplayDialog("Error", "All selected files must be Prefabs.", "OK");
                    return false;
                }
            }

            //Folder
            string targetFolder = Path.Combine("Assets", folderName);
            if (!AssetDatabase.IsValidFolder(targetFolder))
            {
                AssetDatabase.CreateFolder("Assets", folderName);
            }

            //TileSet
            var tileSet = ScriptableObject.CreateInstance<SBO_TileSet>();
            var tiles_so = new SerializedObject(tileSet);
            tiles_so.Update();
            var listProp = tiles_so.FindProperty("tiles");

            listProp.ClearArray();
            listProp.InsertArrayElementAtIndex(0);
            listProp.GetArrayElementAtIndex(0).objectReferenceValue = null;

            var selectedPrefabs = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
            foreach (var go in selectedPrefabs)
            {
                int newIndex = listProp.arraySize;
                listProp.InsertArrayElementAtIndex(newIndex);
                listProp.GetArrayElementAtIndex(newIndex).objectReferenceValue = go;
            }
            tiles_so.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(tileSet, Path.Combine(targetFolder, folderName + "_TileSet.asset"));
            EditorUtility.SetDirty(tileSet);

            //Rules
            var rules = ScriptableObject.CreateInstance<SBO_Rules>();
            rules.tileSet = tileSet;
            rules.Init();
            rules.UpdateTileSet();

            AssetDatabase.CreateAsset(rules, Path.Combine(targetFolder, folderName + "_Rules.asset"));
            EditorUtility.SetDirty(rules);

            sbo_rules = rules;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return true;
        }

        private void SetScenePrefab()
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabToInstanciate);
            instance.transform.position = Vector3.zero;
            instance.GetComponent<SCR_WFC_Solver>().rules = sbo_rules;
            instance.name = "WFC_" + folderName;

            Undo.RegisterCreatedObjectUndo(instance, "PRF_WFC intantiation");

            Selection.activeObject = instance;
        }
    }
}