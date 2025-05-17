using UnityEditor;
using UnityEngine;

namespace PCG_Tool
{

    [ExecuteInEditMode]
    public class SCR_ModelPrinter : MonoBehaviour
    {
        public bool generateOnStart = false;
        public bool updateChanges = false;
        
        public SBO_RepresentationModel model;
        public SBO_TileSet tileSet;
        public Transform parentTransform;

        void Start()
        {
            if (generateOnStart) Generate();
        }

        #region PRINT
        public void Generate()
        {
            PrePrint();
            PrintModel();
        }

        public void ClearTiles()
        {
            if (parentTransform != null) DestroyImmediate(parentTransform.gameObject);
        }

        private void PrePrint()
        {
            if (model == null || model.tileSet == null)
            {
                Debug.LogError("ModelPrinter: RepresentationModel or TileSet is missing.");
                return;
            }

            tileSet = model.tileSet;
        }

        public void PrintModel()
        {
            if (model == null || tileSet == null)
            {
                Debug.LogError("ModelPrinter: RepresentationModel or TileSet is missing.");
                return;
            }

            // Create parent
            ClearTiles();
            GameObject parentObject = new GameObject("Generated Tiles");
            parentTransform = parentObject.transform;
            Undo.RegisterCreatedObjectUndo(parentObject, "Create Tiles Parent");

            // Print
            for (int x = 0; x < model.GridSize.x; x++)
            {
                for (int y = 0; y < model.GridSize.y; y++)
                {
                    for (int z = 0; z < model.GridSize.z; z++)
                    {
                        TileInfo tileInfo = model.GetTile(x, y, z);
                        if (tileInfo.id == 0) continue;  // ID 0 is empty space

                        GameObject prefab = tileSet.GetPrefab(tileInfo.id);
                        if (prefab == null)
                        {
                            Debug.Log("ModelPrinter: Null item detected.");
                            continue;
                        }

                        Vector3 position = new Vector3(x * tileSet.tileSize.x, y * tileSet.tileSize.y, z * tileSet.tileSize.z);
                        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                        instance.transform.position = position;
                        instance.transform.rotation = tileInfo.GetRotation();
                        instance.transform.localScale = tileInfo.GetMirroredScale();
                        instance.transform.parent = parentTransform;
                    }
                }
            }
        }
        #endregion

        #region ON_EDITOR
        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            UnSubscribe();
        }

        //Any time an inspector value changes
        public void OnValidate()
        {
            UpdateSubscription();
        }
        #endregion

        #region ON_UPDATE_SUBSCRIPTION
        private void UpdateSubscription()
        {
            UnSubscribe();
            Subscribe();
        }

        private void Subscribe()
        {
            if(updateChanges && model != null)
            {
                model.OnModelChanged -= Generate; //Prevent double subscription
                model.OnModelChanged += Generate;
            }
        }

        private void UnSubscribe()
        {
            if (model != null)
            {
                model.OnModelChanged -= Generate;
            }
        }
        #endregion
    }

}