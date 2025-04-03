using UnityEditor;
using UnityEngine;

namespace PCG_Tool
{

    public class EDT_SCN_Rules : EditorWindow
    {
        /*private PreviewRenderUtility previewUtility;
        private GameObject tilePreview;
        private Vector2 previewRotation;
        private static GameObject tilePrefab;

        [MenuItem("Tools/Tile Preview")]
        public static void ShowWindow()
        {
            GetWindow<EDT_SCN_Rules>("Tile Preview").Init();
        }

        private void Init()
        {
            if (previewUtility == null)
            {
                previewUtility = new PreviewRenderUtility();
                previewUtility.cameraFieldOfView = 30f;
                previewUtility.camera.transform.position = new Vector3(0, 2, -5);
                previewUtility.camera.transform.rotation = Quaternion.Euler(30, 0, 0);
                //previewUtility.lights[0].intensity = 1.2f;
                //previewUtility.lights[1].intensity = 0.5f;
            }

            if (tilePrefab != null)
            {
                if (tilePreview != null)
                {
                    Object.DestroyImmediate(tilePreview);
                }

                tilePreview = (GameObject)Instantiate(tilePrefab);
                //tilePreview.hideFlags = HideFlags.HideAndDontSave;
                tilePreview.GetComponent<Renderer>().sharedMaterial = tilePrefab.GetComponent<Renderer>().sharedMaterial;
                previewUtility.AddSingleGO(tilePreview);
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Tile Preview", EditorStyles.boldLabel);

            tilePrefab = (GameObject)EditorGUILayout.ObjectField("Tile Prefab", tilePrefab, typeof(GameObject), false);

            if (GUILayout.Button("Actualizar Vista Previa"))
            {
                Init();
            }

            if (previewUtility == null || tilePreview == null) return;

            Rect previewRect = GUILayoutUtility.GetRect(256, 256, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            // Rotación con el Mouse
            if (Event.current.type == EventType.MouseDrag && previewRect.Contains(Event.current.mousePosition))
            {
                previewRotation += Event.current.delta;
                Event.current.Use();
            }

            // Renderizado
            previewUtility.BeginPreview(previewRect, GUIStyle.none);
            tilePreview.transform.rotation = Quaternion.Euler(previewRotation.y, -previewRotation.x, 0);
            previewUtility.camera.transform.LookAt(tilePreview.transform.position);
            previewUtility.Render();
            Texture previewTexture = previewUtility.EndPreview();
            GUI.DrawTexture(previewRect, previewTexture, ScaleMode.StretchToFill);
        }

        private void OnDestroy()
        {
            previewUtility?.Cleanup();
            if (tilePreview != null) DestroyImmediate(tilePreview);
        }

        private void OnDisable()
        {
            previewUtility?.Cleanup();
            if (tilePreview != null) DestroyImmediate(tilePreview);
        }
        */

        private static EDT_SCN_Rules window;
        private Camera sceneCamera;
        private RenderTexture renderTexture;

        private GameObject tilePreview;  // Objeto instanciado del prefab
        private Material tileMaterial;
        private static GameObject tilePrefab; // Prefab asignado por el usuario

        //Scene movement
        private float moveSpeed = 2f;
        private float zoomSpeed = 2f;
        private float rotationSpeed = 1f;
        private bool isRightMouseDown = false;
        private Vector2 lastMousePosition;

        //[MenuItem("Tools/Tile Painter")]
        public static void ShowWindow()
        {
            window = GetWindow<EDT_SCN_Rules>("Tile Painter");
            window.Init();
        }

        private void Init()
        {
            // Crear una camara separada para esta ventana
            if (sceneCamera == null)
            {
                GameObject cameraObj = new GameObject("TilePainterCamera");
                //cameraObj.hideFlags = HideFlags.HideAndDontSave;
                sceneCamera = cameraObj.AddComponent<Camera>();
                sceneCamera.clearFlags = CameraClearFlags.SolidColor;
                sceneCamera.backgroundColor = Color.gray;
                sceneCamera.orthographic = false;
                sceneCamera.fieldOfView = 60;
                sceneCamera.cullingMask = 1 << 31; // Solo renderiza layer 31
            }

            UpdateRenderTexture();

            // Si ya existe una tile, la eliminamos antes de instanciar otra
            if (tilePreview != null)
            {
                DestroyImmediate(tilePreview);
            }

            if(tilePrefab != null)
            {
                // Instanciar el prefab sin agregarlo a la jerarquía
                tilePreview = Instantiate(tilePrefab);
                //tilePreview.hideFlags = HideFlags.HideAndDontSave;
                tilePreview.transform.position = Vector3.zero;
                tilePreview.layer = 31; // Solo esta layer será renderizada

                // Obtener el material de la tile
                Renderer renderer = tilePreview.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    tileMaterial = renderer.sharedMaterial;
                }
            }

            EditorApplication.update -= UpdateCamera;
            EditorApplication.update += UpdateCamera;
        }

        private void OnGUI()
        {
            if (sceneCamera == null)
            {
                Init();
            }

            GUILayout.Label("Tile Painter", EditorStyles.boldLabel);

            // Selector de Prefab
            tilePrefab = (GameObject)EditorGUILayout.ObjectField("Tile Prefab", tilePrefab, typeof(GameObject), false);

            if (GUILayout.Button("Update Scene"))
            {
                Init(); // Reiniciar la preview con el prefab seleccionado
            }

            // Mostrar la vista de la cámara en la ventana
            if (renderTexture != null)
            {
                Rect lastRect = GUILayoutUtility.GetLastRect();
                float remainingHeight = position.height - lastRect.yMax - 10;
                if (remainingHeight > 0)
                {
                    GUILayout.BeginArea(new Rect(5, lastRect.yMax + 5, position.width - 10, remainingHeight));
                    GUI.DrawTexture(new Rect(0, 0, position.width - 10, remainingHeight), renderTexture, ScaleMode.StretchToFill);
                    GUILayout.EndArea();
                }
            }

            UpdateCamera();
        }

        private void UpdateRenderTexture()
        {
            // Obtener el tamaño de la ventana y ajustar la resolución
            if (renderTexture != null)
            {
                renderTexture.Release();
                DestroyImmediate(renderTexture);
            }

            int width = Mathf.Max(256, (int)position.width - 10);
            int height = Mathf.Max(256, (int)position.height - 50);

            renderTexture = new RenderTexture(width, height, 16);
            sceneCamera.targetTexture = renderTexture;
        }

        private void ClearRenderTexture()
        {
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, Color.gray);
            RenderTexture.active = null;
        }

        private void OnDestroy()
        {
            if (sceneCamera != null) DestroyImmediate(sceneCamera.gameObject);
            if (tilePreview != null) DestroyImmediate(tilePreview);
            EditorApplication.update -= UpdateCamera;
        }

        private void UpdateCamera()
        {
            if (sceneCamera == null) return;

            Event e = Event.current;
            if (e == null) return;

            // Captura del teclado para movimiento
            if (e.type == EventType.KeyDown)
            {
                Vector3 movement = Vector3.zero;
                if (e.keyCode == KeyCode.W)
                {
                    movement += sceneCamera.transform.forward;
                    Debug.Log("W");
                }
                if (e.keyCode == KeyCode.S) movement -= sceneCamera.transform.forward;
                if (e.keyCode == KeyCode.A) movement -= sceneCamera.transform.right;
                if (e.keyCode == KeyCode.D) movement += sceneCamera.transform.right;
                if (e.keyCode == KeyCode.Q) movement -= sceneCamera.transform.up;
                if (e.keyCode == KeyCode.E) movement += sceneCamera.transform.up;

                sceneCamera.transform.position += movement * moveSpeed;
                e.Use();
            }

            // Zoom con la rueda del ratón
            if (e.type == EventType.ScrollWheel)
            {
                float scroll = e.delta.y;
                sceneCamera.transform.position += sceneCamera.transform.forward * scroll * zoomSpeed;
                e.Use();
            }

            // Rotación con el botón derecho del ratón
            if (e.type == EventType.MouseDown && e.button == 1)
            {
                isRightMouseDown = true;
                lastMousePosition = e.mousePosition;
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 1)
            {
                isRightMouseDown = false;
                e.Use();
            }

            if (isRightMouseDown && e.type == EventType.MouseDrag)
            {
                Vector2 delta = e.mousePosition - lastMousePosition;
                lastMousePosition = e.mousePosition;

                float rotX = delta.x * rotationSpeed;
                float rotY = delta.y * rotationSpeed; // Invertido correctamente

                sceneCamera.transform.Rotate(Vector3.up, rotX, Space.World);
                sceneCamera.transform.Rotate(sceneCamera.transform.right, rotY, Space.World);
                e.Use();
            }
        }
    }

}
