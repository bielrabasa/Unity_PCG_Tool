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
        Material selectedColorMat;
        private const float GHOST_FACE_ALPHA = 0.4f;
        private float ghostFaceSeparation = 0.2f;

        //Color picker
        private int selectedColorIndex = 0;
        private static Texture2D[] colorTextures;

        //Constraint selection
        private bool constraintMenu = false;
        private Vector3[] constraintPositions;

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

            //Previewing Menu 
            if (isPreviewing)
            {
                //Constraint and Color picking menus
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = !constraintMenu ? STY_Style.Activated_Color : STY_Style.Deactivated_Color;
                if (GUILayout.Button("Color Picking", STY_Style.Button_Layout))
                {
                    if(constraintMenu) constraintMenu = false;
                    RefreshTilePreview();
                }

                GUI.backgroundColor = constraintMenu ? STY_Style.Activated_Color : STY_Style.Deactivated_Color;
                if (GUILayout.Button("Set Constraints", STY_Style.Button_Layout))
                {
                    if (!constraintMenu) constraintMenu = true;
                    RefreshTilePreview();
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.Space(20);

                GUI.backgroundColor = STY_Style.Variable_Color;
                float newTilePreviewSeparation = EditorGUILayout.Slider("Tile Separation", tilePreviewSeparation, 0f, 10f);
                float newGhostFaceSeparation = constraintMenu? ghostFaceSeparation : EditorGUILayout.Slider("GhostFace Separation", ghostFaceSeparation, 0f, 1f);
                
                if (tilePreviewSeparation != newTilePreviewSeparation || ghostFaceSeparation != newGhostFaceSeparation)
                {
                    tilePreviewSeparation = newTilePreviewSeparation;
                    ghostFaceSeparation = newGhostFaceSeparation;
                    RefreshTilePreview();
                }
            }

            //Color compatibility matrix
            InspectorColorCompatibilityMatrix();

            EditorGUILayout.Space(20);

            GUI.backgroundColor = STY_Style.Variable_Color;
            if (GUILayout.Button("Reset Rules", STY_Style.Button_Layout))
            {
                if (EditorUtility.DisplayDialog("Resetting rules...", 
                    "Are you sure you want to reset all the rules?",
                    "Yes, Reset",
                    "Cancel"))
                {
                    UpdateTileSet(); //Not the same but useful
                }
            }
        }

        private void RefreshTilePreview()
        {
            CleanupPreview(cleanCamera: false);
            ShowTilesPreview(setupCamera: false);
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

            if (constraintMenu)
            {
                SetupContraintMenu();
            }
            else
            {
                SetupGhostFaceData();
            }

            if (setupCamera) SetupCamera();

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
            _previewParent.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy | HideFlags.NotEditable;

            //Calculate tile distribution
            Vector3 tileSize = tileSet.tileSize;
            int rowSize = Mathf.CeilToInt(Mathf.Sqrt(tileSet.GetTileCount()));
            tilePreviewCenter = new Vector3((rowSize - 1) * (tileSize.x + tilePreviewSeparation) / 2f, 0, (rowSize - 1) * (tileSize.z + tilePreviewSeparation) / 2f);

            //Create tilePositions
            constraintPositions = new Vector3[tileSet.GetTileCount()];

            //Intanciate tiles
            for (int i = 0; i < tileSet.GetTileCount(); i++)
            {
                GameObject prefab = tileSet.GetPrefab((short)(i));
                if (prefab == null) continue;

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy | HideFlags.NotEditable;
                instance.transform.position = new Vector3((i % rowSize) * (tileSize.x + tilePreviewSeparation), 0, (i / rowSize) * (tileSize.z + tilePreviewSeparation));
                instance.transform.SetParent(_previewParent.transform);
                instance.layer = _previewLayer;

                SceneVisibilityManager.instance.DisablePicking(instance, true);

                foreach (Transform child in instance.transform)
                    child.gameObject.layer = _previewLayer;

                //Generate ghost faces
                GenerateFacesForTile((short)i, instance.transform.position, tileSet.tileSize);

                //Generation tile positions for constraints
                constraintPositions[i] = instance.transform.position + Vector3.up * tileSet.tileSize.y;
            }
        }

        private void SetupContraintMenu()
        {
            SceneView.duringSceneGui -= DrawConstraintsGUI;
            SceneView.duringSceneGui += DrawConstraintsGUI;
        }

        private void DrawConstraintsGUI(SceneView sv)
        {
            if(constraintPositions == null || constraintPositions.Length == 0) return;

            for (int i = 0; i < constraintPositions.Length; i++)
            {
                TileRule rule = rules.tileRules[i];

                Handles.BeginGUI();

                //Convert world pos to screen pos
                Vector2 guiPos = HandleUtility.WorldToGUIPoint(constraintPositions[i]);
                
                GUILayout.BeginArea(new Rect(guiPos.x - 30, guiPos.y - 75, 60, 75), GUI.skin.box);
                bool newAllowMirroring = GUILayout.Toggle((rule.constraints & TileConstraints.AllowMirror) != 0, "Mirror");
                bool newAllowRotX = GUILayout.Toggle((rule.constraints & TileConstraints.Allow_X_Rotation) != 0, "X rot");
                bool newAllowRotY = GUILayout.Toggle((rule.constraints & TileConstraints.Allow_Y_Rotation) != 0, "Y rot");
                bool newAllowRotZ = GUILayout.Toggle((rule.constraints & TileConstraints.Allow_Z_Rotation) != 0, "Z rot");
                rule.constraints =
                    (newAllowMirroring ? TileConstraints.AllowMirror : 0) |
                    (newAllowRotX ? TileConstraints.Allow_X_Rotation : 0) |
                    (newAllowRotY ? TileConstraints.Allow_Y_Rotation : 0) |
                    (newAllowRotZ ? TileConstraints.Allow_Z_Rotation : 0);
                GUILayout.EndArea();

                Handles.EndGUI();

                rules.tileRules[i] = rule;
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
            SceneView.duringSceneGui -= DrawConstraintsGUI;

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

            for (int i = 0; i < normals.Length; i++) {
                var normal = normals[i];
                Vector3 facePos = tilePos + Vector3.Scale(normal, half);
                ghostFaces.Add(new EDT_GUI_FacePreview(facePos + (normal * ghostFaceSeparation), normal, tileId, (FaceDirection)i));
            }
        }

        void GhostFacePaintGUI(SceneView sv)
        {
            DrawColorPicker();

            //Draw ghostfaces
            Event e = Event.current;
            Ray ray = new Ray();
            bool click = false;
            if(e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                click = true;
            }

            if (ghostFaceMesh == null || ghostFaceMat == null || ghostFaces == null) return;

            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            //Ensure selectedColorMaterial is correct
            ProcessColorChange(selectedColorIndex);

            //GhostFace Draw & Click
            EDT_GUI_FacePreview closestFace = null;
            float closestDistance = float.MaxValue;

            foreach (var face in ghostFaces)
            {
                //Has the selected color
                if (rules.tileRules[face.ownerId].HasColor(face.dir, TileRule.GetTileColorByIndex(selectedColorIndex)))
                {
                    selectedColorMat.SetPass(0);
                }
                else
                {
                    ghostFaceMat.SetPass(0);
                }

                //Draw face
                Vector3 faceScale = EDT_GUI_FacePreview.GetScaleForNormal(face.rotation * Vector3.forward, rules.tileSet.tileSize);
                Matrix4x4 matrix = Matrix4x4.TRS(face.position, face.rotation, faceScale);
                Graphics.DrawMeshNow(ghostFaceMesh, matrix);

                //Check closest click
                if (click && face.IsHitByRay(ray, rules.tileSet.tileSize, out float distance))
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestFace = face;
                    }
                }
            }

            //Process click for closest face
            if (closestFace != null)
            {
                ProcessTileClick(closestFace);
                e.Use();
            }
        }

        void DrawColorPicker()
        {
            Handles.BeginGUI();

            float buttonSize = 24f;
            float spacing = 4f;
            float totalWidth = (buttonSize + spacing) * SBO_Rules.MATRIX_COLOR_COUNT - spacing;
            float startX = (Screen.width - totalWidth) * 0.5f;
            float y = 10f;

            GUILayout.BeginArea(new Rect(startX, y, totalWidth, buttonSize));
            GUILayout.BeginHorizontal();

            EnsureColorTextures();

            for (int i = 0; i < SBO_Rules.MATRIX_COLOR_COUNT; i++)
            {
                Rect rect = GUILayoutUtility.GetRect(buttonSize, buttonSize, GUILayout.ExpandWidth(false));

                //Button texture
                GUI.DrawTexture(rect, colorTextures[i]);

                //If selected, draw border
                if (selectedColorIndex == i)
                {
                    Color borderColor = Color.white;
                    float thickness = 2f;

                    //Top
                    EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), borderColor);
                    //Bottom
                    EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), borderColor);
                    //Left
                    EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), borderColor);
                    //Right
                    EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), borderColor);
                }

                //Invisible button on top to detect click
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                {
                    ProcessColorChange(i);
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            Handles.EndGUI();
        }

        private void EnsureColorTextures()
        {
            if (colorTextures != null) return;

            colorTextures = new Texture2D[SBO_Rules.MATRIX_COLOR_COUNT];

            colorTextures[0] = SCR_CheckerTextureUtility.GetCheckerTexture();

            for (int i = 1; i < SBO_Rules.MATRIX_COLOR_COUNT; i++)
            {
                Color col = TileRule.GetColor(i);
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, col);
                tex.Apply();
                colorTextures[i] = tex;
            }
        }

        private void ProcessColorChange(int buttonIndex)
        {
            selectedColorIndex = buttonIndex;

            //Change Material Information
            if (selectedColorMat == null)
            {
                selectedColorMat = new Material(ghostFaceMat);
            }

            Color col = TileRule.GetColor(selectedColorIndex);
            col.a = GHOST_FACE_ALPHA;
            selectedColorMat.color = col;
        }

        void ProcessTileClick(EDT_GUI_FacePreview face)
        {
            rules.tileRules[face.ownerId].SwitchColorToFace(face.dir, TileRule.GetTileColorByIndex(selectedColorIndex));
        }

        void InspectorColorCompatibilityMatrix()
        {
            GUI.backgroundColor = STY_Style.Variable_Color;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Color Compatibility Matrix", EditorStyles.boldLabel);

            SBO_Rules rules = (SBO_Rules)target;

            EnsureColorTextures();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(14)); //Empty space before colors

            for (int x = 0; x < SBO_Rules.MATRIX_COLOR_COUNT; x++)
            {
                Rect rect = GUILayoutUtility.GetRect(18, 15, GUILayout.Width(18), GUILayout.Height(15));
                rect.width -= 3;

                EditorGUI.DrawPreviewTexture(rect, colorTextures[x]);
            }
            EditorGUILayout.EndHorizontal();

            for (int y = 0; y < SBO_Rules.MATRIX_COLOR_COUNT; y++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical(GUILayout.Width(15));
                GUILayout.Space(3);
                Rect rect = GUILayoutUtility.GetRect(15, 15, GUILayout.Width(15), GUILayout.Height(15));
                EditorGUI.DrawPreviewTexture(rect, colorTextures[y]);
                EditorGUILayout.EndVertical();


                for (int x = 0; x < SBO_Rules.MATRIX_COLOR_COUNT; x++)
                {
                    if(x == 0 && y == 0)
                    {
                        GUI.enabled = false;
                        EditorGUILayout.Toggle(true, GUILayout.Width(15));
                        GUI.enabled = true;

                        continue;
                    }

                    bool compatible = rules.AreCompatible((TileColor)(1 << x), (TileColor)(1 << y));
                    bool newCompatible = EditorGUILayout.Toggle(compatible, GUILayout.Width(15));

                    if (newCompatible != compatible)
                    {
                        Undo.RecordObject(rules, "Toggle Color Compatibility");
                        rules.SwitchCompatibility((TileColor)(1 << x), (TileColor)(1 << y));
                        EditorUtility.SetDirty(rules);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }

}
