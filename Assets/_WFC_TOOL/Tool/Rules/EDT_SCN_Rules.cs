using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PCG_Tool
{

    [CustomEditor(typeof(SBO_Rules))]
    public class EDT_SCN_Rules : Editor
    {
        SBO_Rules rules;
        private GameObject _previewParent;
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

        //Ghost faces
        List<EDT_GUI_FacePreview> ghostFaces = new();
        Mesh ghostFaceMesh;
        [SerializeField] Material ghostFaceMat;
        private float ghostFaceSeparation = 0.2f;

        //Color picker
        private int selectedColorIndex = 0;
        private static Texture2D[] colorTextures;

        public override void OnInspectorGUI()
        {
            rules = (SBO_Rules)target;

            EditorGUILayout.LabelField("Rules", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawTileSetInspector();

            EditorGUILayout.Space(20);

            //Button
            GUI.backgroundColor = isPreviewing? STY_Style.Activated_Color : STY_Style.Deactivated_Color;
            if (GUILayout.Button(isPreviewing ? "Hide Tile Preview" : "Show Tile Preview", STY_Style.Button_Layout))
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
                EditorGUILayout.Space(20);

                GUI.backgroundColor = STY_Style.Variable_Color;
                float newTilePreviewSeparation = EditorGUILayout.Slider("Tile Separation", tilePreviewSeparation, 0f, 10f);
                float newGhostFaceSeparation = EditorGUILayout.Slider("GhostFace Separation", ghostFaceSeparation, 0f, 1f);
                
                if (tilePreviewSeparation != newTilePreviewSeparation || ghostFaceSeparation != newGhostFaceSeparation)
                {
                    tilePreviewSeparation = newTilePreviewSeparation;
                    ghostFaceSeparation = newGhostFaceSeparation;
                    CleanupPreview(cleanCamera: false);
                    ShowTilesPreview(setupCamera: false);
                }
            }
        }

        private void DrawTileSetInspector()
        {
            SBO_TileSet newTileSet = (SBO_TileSet)EditorGUILayout.ObjectField("Tile Set", rules.tileSet, typeof(SBO_TileSet), false);

            //If is the same, don't change anything
            if (newTileSet == rules.tileSet) return;
            //Changing something
            
            //1st Assign
            if(rules.tileSet == null && newTileSet != null)
            {
                rules.tileSet = newTileSet;
                UpdateTileSet();
                return;
            }

            //Changing tileSet with same size
            if (newTileSet != null && rules.tileSet.GetTileCount() == newTileSet.GetTileCount())
            {
                if (EditorUtility.DisplayDialog(
                        "Changing TileSet...",
                        "Changing TileSet with the same Tile count will match rule data by Id. Are you sure you want to do it?",
                        "Yes, change",
                        "Cancel"))
                {
                    rules.tileSet = newTileSet;
                }
                return;
            }

            //If changing tileSet CAUTION
            if (EditorUtility.DisplayDialog(
                    "Changing TileSet...",
                    "Changing TileSet with different Tile count will erase all rules data. Are you sure you want to do it?",
                    "Yes, change",
                    "Cancel"))
            {
                rules.tileSet = newTileSet;
                UpdateTileSet();
                return;
            }
        }

        private void UpdateTileSet()
        {
            if(rules.tileSet == null) return;

            rules.tileRules = new TileRule[rules.tileSet.GetTileCount()];

            EditorUtility.SetDirty(rules);
        }

        private void ShowTilesPreview(bool setupCamera = true)
        {
            if (rules.tileSet == null)
            {
                Debug.LogWarning("Rules: No TileSet asigned.");
                return;
            }

            //Open Scene if needed
            if (SceneView.lastActiveSceneView == null)
            {
                Debug.Log("Tile Preview is done in the scene viewport.");
                EditorWindow.CreateWindow<SceneView>();
            }

            SetupObjects(rules.tileSet);

            SetupGhostFaceData();

            if(setupCamera) SetupCamera();

            isPreviewing = true;

            SCR_InspectorLockUtility.SetInspectorLock(true);

            EditorWindow.FocusWindowIfItsOpen<SceneView>();
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
            tilePreviewCenter = new Vector3((rowSize - 1) * (tileSize.x + tilePreviewSeparation) / 2f, 0, (rowSize - 1) * (tileSize.z + tilePreviewSeparation) / 2f);

            //Intanciate tiles
            for (int i = 0; i < tileSet.GetTileCount(); i++)
            {
                GameObject prefab = tileSet.GetPrefab((short)(i));
                if (prefab == null) continue;

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
                instance.transform.position = new Vector3((i % rowSize) * (tileSize.x + tilePreviewSeparation), 0, (i / rowSize) * (tileSize.z + tilePreviewSeparation));
                instance.transform.SetParent(_previewParent.transform);
                instance.layer = _previewLayer;

                SceneVisibilityManager.instance.DisablePicking(instance, true);

                foreach (Transform child in instance.transform)
                    child.gameObject.layer = _previewLayer;

                //Generate ghost faces
                GenerateFacesForTile((short)i, instance.transform.position, tileSet.tileSize);
            }
        }

        private void SetupGhostFaceData()
        {
            ghostFaceMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");

            if (ghostFaceMat == null)
            {
                Debug.LogError("GhostFace material not set!");
                return;
            }

            SceneView.duringSceneGui -= GhostFacePaintGUI;
            SceneView.duringSceneGui += GhostFacePaintGUI;
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

        private void CleanupPreview(bool cleanCamera = true)
        {
            if (_previewParent != null)
            {
                DestroyImmediate(_previewParent);
            }
            ghostFaces.Clear();
            SceneView.duringSceneGui -= GhostFacePaintGUI;

            if(cleanCamera)
            {
                if (sceneView != null)
                {
                    //Original Layer
                    Tools.visibleLayers = originalLayerMask;

                    //Original Transform
                    sceneView.LookAtDirect(originalCameraPivot, originalCameraPivotRotation, originalCameraZoom);

                    sceneView.Repaint();
                }
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

        void GenerateFacesForTile(short tileId, Vector3 tilePos, Vector3 tileSize)
        {
            Vector3 half = tileSize * 0.5f;

            Vector3[] normals = new Vector3[]
            {
                Vector3.up,
                Vector3.down,
                Vector3.left,
                Vector3.right,
                Vector3.forward,
                Vector3.back
            };

            foreach (var normal in normals)
            {
                Vector3 facePos = tilePos + Vector3.Scale(normal, half);
                ghostFaces.Add(new EDT_GUI_FacePreview(facePos + (normal * ghostFaceSeparation), normal, tileId));
            }
        }

        void GhostFacePaintGUI(SceneView sv)
        {
            DrawColorPicker();

            //Draw ghostfaces
            Event e = Event.current;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (ghostFaceMesh == null || ghostFaceMat == null || ghostFaces == null) return;

            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            ghostFaceMat.SetPass(0);

            foreach (var face in ghostFaces)
            {
                //Color from pick TODO: Only if they have it
                //ghostFaceMat.color = TileRule.GetColor(selectedColorIndex);

                Vector3 faceScale = EDT_GUI_FacePreview.GetScaleForNormal(face.rotation * Vector3.forward, rules.tileSet.tileSize);
                Matrix4x4 matrix = Matrix4x4.TRS(face.position, face.rotation, faceScale);
                Graphics.DrawMeshNow(ghostFaceMesh, matrix);

                if (e.type == EventType.MouseDown && e.button == 0 && !e.alt && face.IsHitByRay(ray, rules.tileSet.tileSize))
                {
                    ProcessTileClick(face);
                    e.Use();
                    break;
                }
            }
        }

        void DrawColorPicker()
        {
            Handles.BeginGUI();

            float buttonSize = 24f;
            float spacing = 4f;
            int colorCount = 16;
            float totalWidth = (buttonSize + spacing) * colorCount - spacing;
            float startX = (Screen.width - totalWidth) * 0.5f;
            float y = 10f;

            GUILayout.BeginArea(new Rect(startX, y, totalWidth, buttonSize));
            GUILayout.BeginHorizontal();

            EnsureColorTextures();

            for (int i = 0; i < colorCount; i++)
            {
                Rect rect = GUILayoutUtility.GetRect(buttonSize, buttonSize, GUILayout.ExpandWidth(false));

                // Fondo del botón: el color de la tile
                GUI.DrawTexture(rect, colorTextures[i]);

                // Si está seleccionado, dibujamos borde
                if (selectedColorIndex == i)
                {
                    Color borderColor = Color.white;
                    float thickness = 2f;

                    // Top
                    EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), borderColor);
                    // Bottom
                    EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), borderColor);
                    // Left
                    EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), borderColor);
                    // Right
                    EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), borderColor);
                }

                // Botón invisible encima para detección de clics
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                {
                    selectedColorIndex = i;
                }
            }

            GUI.backgroundColor = Color.white; // Reset color
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            Handles.EndGUI();
        }

        private void EnsureColorTextures()
        {
            if (colorTextures != null) return;

            colorTextures = new Texture2D[16];

            colorTextures[0] = SCR_CheckerTextureUtility.GetCheckerTexture();

            for (int i = 1; i < 16; i++)
            {
                Color col = TileRule.GetColor(i);
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, col);
                tex.Apply();
                colorTextures[i] = tex;
            }
        }

        void ProcessTileClick(EDT_GUI_FacePreview face)
        {
            //TODO
            Debug.Log("TileClicked: " + face.ownerId);
        }
    }

}
