using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace PCG_Tool
{

    [CustomEditor(typeof(SBO_Rules))]
    public class EDT_SCN_Rules : Editor
    {
        GameObject _previewParent;
        private const int _previewLayer = 31;
        private bool isPreviewing = false;

        //Original Scene
        private SceneView sceneView;
        private int originalLayerMask;
        private Vector3 originalCameraPivot;
        private Quaternion originalCameraPivotRotation;
        private float originalCameraZoom;

        //Tile positions
        private float tilePreviewSeparation = 2;
        private Vector3 tilePreviewCenter;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            //Button
            if (GUILayout.Button(isPreviewing ? "Ocultar Tiles Preview" : "Mostrar Tiles Preview"))
            {
                if (isPreviewing)
                {
                    CleanupPreview();
                    SCR_InspectorLockUtility.SetInspectorLock(false);
                }
                else
                    ShowTilesPreview();
            }

            //TileSeparation 
            if (isPreviewing)
            {
                float newTilePreviewSeparation = EditorGUILayout.Slider("Tile Separation", tilePreviewSeparation, 0f, 10f);
                if (tilePreviewSeparation != newTilePreviewSeparation)
                {
                    tilePreviewSeparation = newTilePreviewSeparation;
                    CleanupPreview();
                    ShowTilesPreview();
                }
            }
        }

        private void ShowTilesPreview()
        {
            SBO_Rules rules = (SBO_Rules)target;
            SBO_TileSet tileSet = rules.tileSet;
            if (tileSet == null)
            {
                Debug.LogWarning("Rules: No TileSet asignado.");
                return;
            }

            SetupObjects(tileSet);

            SetupCamera();

            isPreviewing = true;

            SCR_InspectorLockUtility.SetInspectorLock(true);
        }

        private void SetupObjects(SBO_TileSet tileSet)
        {
            //Create parent
            if(_previewParent != null)
            {
                DestroyImmediate(_previewParent);
            }
            _previewParent = new GameObject("TilePreview_EditorOnly");
            _previewParent.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;

            //Calculate tile distribution
            Vector3 tileSize = tileSet.tileSize;
            int rowSize = Mathf.CeilToInt(Mathf.Sqrt(tileSet.GetTileCount()));
            tilePreviewCenter = new Vector3((rowSize / 2f) * tileSize.x * tilePreviewSeparation, 0, (rowSize / 2f) * tileSize.z * tilePreviewSeparation);

            //Intanciate tiles
            for (int i = 0; i < tileSet.GetTileCount(); i++)
            {
                GameObject prefab = tileSet.GetPrefab((short)(i));
                if (prefab == null) continue;

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.hideFlags = HideFlags.DontSave;
                instance.transform.position = new Vector3((i % rowSize) * tileSize.x * tilePreviewSeparation, 0, (i / rowSize) * tileSize.z * tilePreviewSeparation);
                instance.transform.SetParent(_previewParent.transform);
                instance.layer = _previewLayer;

                foreach (Transform child in instance.transform)
                    child.gameObject.layer = _previewLayer;
            }
        }

        private void SetupCamera()
        {
            sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                //Layer
                originalLayerMask = Tools.visibleLayers;
                Tools.visibleLayers = 1 << _previewLayer;

                //Transform
                originalCameraPivot = sceneView.pivot;
                originalCameraPivotRotation = sceneView.rotation;
                originalCameraZoom = sceneView.size;

                sceneView.LookAtDirect(tilePreviewCenter, Quaternion.Euler(45, 45, 0), 5f);

                sceneView.Repaint();
            }
        }

        private void CleanupPreview()
        {
            if (_previewParent != null)
            {
                DestroyImmediate(_previewParent);
            }

            if (sceneView != null)
            {
                //Original Layer
                Tools.visibleLayers = originalLayerMask;

                //Original Transform
                sceneView.LookAtDirect(originalCameraPivot, originalCameraPivotRotation, originalCameraZoom);

                sceneView.Repaint();
            }

            isPreviewing = false;
        }

        private void OnDisable()
        {
            if (isPreviewing)
            {
                CleanupPreview();
                SCR_InspectorLockUtility.SetInspectorLock(false);
            }
        }
    }

}
