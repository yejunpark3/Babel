using UnityEngine;

namespace HexTileMap
{
    /// <summary>
    /// Simple scene setup script for testing the hex tile map system
    /// Add this to any GameObject in the scene to automatically set up a basic hex tile map
    /// </summary>
    public class HexTileMapSceneSetup : MonoBehaviour
    {
        [Header("Setup Options")]
        [SerializeField] private bool setupOnStart = true;
        [SerializeField] private bool createTileMapManager = true;
        [SerializeField] private bool createInteraction = true;
        [SerializeField] private bool createDemo = true;

        [Header("Map Settings")]
        [SerializeField] private Vector2Int mapSize = new Vector2Int(8, 8);
        [SerializeField] private float hexSize = 1.0f;

        [Header("Camera Setup")]
        [SerializeField] private bool adjustCamera = true;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0, 10, -5);

        private void Start()
        {
            if (setupOnStart)
            {
                SetupHexTileMapSystem();
            }
        }

        /// <summary>
        /// Sets up the complete hex tile map system
        /// </summary>
        [ContextMenu("Setup Hex Tile Map System")]
        public void SetupHexTileMapSystem()
        {
            // Create main tile map manager
            GameObject tileMapGO = null;
            HexTileMapManager tileMapManager = null;

            if (createTileMapManager)
            {
                tileMapGO = new GameObject("HexTileMap");
                tileMapManager = tileMapGO.AddComponent<HexTileMapManager>();
                
                // Configure tile map settings using reflection since the fields are private
                var mapSizeField = typeof(HexTileMapManager).GetField("mapSize", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var hexSizeField = typeof(HexTileMapManager).GetField("hexSize", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (mapSizeField != null) mapSizeField.SetValue(tileMapManager, mapSize);
                if (hexSizeField != null) hexSizeField.SetValue(tileMapManager, hexSize);

                Debug.Log("Created HexTileMapManager");
            }
            else
            {
                tileMapManager = FindObjectOfType<HexTileMapManager>();
                if (tileMapManager != null)
                    tileMapGO = tileMapManager.gameObject;
            }

            // Add interaction component
            if (createInteraction && tileMapGO != null)
            {
                var interaction = tileMapGO.GetComponent<HexTileMapInteraction>();
                if (interaction == null)
                {
                    interaction = tileMapGO.AddComponent<HexTileMapInteraction>();
                    Debug.Log("Added HexTileMapInteraction");
                }
            }

            // Add demo component
            if (createDemo && tileMapGO != null)
            {
                var demo = tileMapGO.GetComponent<HexTileMapDemo>();
                if (demo == null)
                {
                    demo = tileMapGO.AddComponent<HexTileMapDemo>();
                    Debug.Log("Added HexTileMapDemo");
                }
            }

            // Setup camera
            if (adjustCamera)
            {
                SetupCamera();
            }

            // Create basic materials
            CreateBasicMaterials();

            Debug.Log("Hex Tile Map System setup complete!");
        }

        /// <summary>
        /// Sets up the camera for optimal hex map viewing
        /// </summary>
        private void SetupCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                camera = FindObjectOfType<Camera>();
            }

            if (camera != null)
            {
                // Position camera to view the center of the map
                camera.transform.position = cameraOffset;
                camera.transform.LookAt(Vector3.zero);
                
                // For 2D-style viewing, use orthographic camera
                if (!camera.orthographic)
                {
                    camera.orthographic = true;
                    camera.orthographicSize = Mathf.Max(mapSize.x, mapSize.y) * hexSize * 0.75f;
                }

                Debug.Log("Camera positioned for hex map viewing");
            }
        }

        /// <summary>
        /// Creates basic materials for tile visualization
        /// </summary>
        private void CreateBasicMaterials()
        {
            // Create a basic material directory if it doesn't exist
            var materialsPath = "Assets/Materials";
            
            // Since we can't directly create Unity assets in runtime, we'll log what should be created
            Debug.Log("To complete setup, create the following materials in " + materialsPath + ":");
            Debug.Log("- HexTileDefault.mat (Standard shader, white color)");
            Debug.Log("- HexTileHighlight.mat (Standard shader, yellow color, emission)");
            Debug.Log("- HexTileSelected.mat (Standard shader, cyan color, emission)");
            Debug.Log("- HexTilePath.mat (Standard shader, red color, emission)");
        }

        /// <summary>
        /// Creates a simple UI for testing (requires Canvas)
        /// </summary>
        [ContextMenu("Create Simple UI")]
        public void CreateSimpleUI()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                
                // Add EventSystem if not present
                if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    var eventSystemGO = new GameObject("EventSystem");
                    eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }

            // Create info panel
            var infoPanelGO = new GameObject("InfoPanel");
            infoPanelGO.transform.SetParent(canvas.transform, false);
            
            var infoRect = infoPanelGO.AddComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0, 0);
            infoRect.anchorMax = new Vector2(0.3f, 1);
            infoRect.offsetMin = Vector2.zero;
            infoRect.offsetMax = Vector2.zero;

            var infoImage = infoPanelGO.AddComponent<UnityEngine.UI.Image>();
            infoImage.color = new Color(0, 0, 0, 0.7f);

            // Create info text
            var infoTextGO = new GameObject("InfoText");
            infoTextGO.transform.SetParent(infoPanelGO.transform, false);
            
            var textRect = infoTextGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);

            var infoText = infoTextGO.AddComponent<UnityEngine.UI.Text>();
            infoText.text = "육각형 타일 맵 시스템\n\n조작법:\n• 좌클릭: 타일 선택\n• P키: 페인트 모드\n• C키: 초기화";
            infoText.color = Color.white;
            infoText.fontSize = 14;
            
            // Try to assign a default font
            infoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            Debug.Log("Simple UI created");
        }

        /// <summary>
        /// Quick test of the pathfinding system
        /// </summary>
        [ContextMenu("Test Pathfinding")]
        public void TestPathfinding()
        {
            var tileMap = FindObjectOfType<HexTileMapManager>();
            if (tileMap == null)
            {
                Debug.LogWarning("No HexTileMapManager found. Please setup the system first.");
                return;
            }

            if (tileMap.Tiles.Count == 0)
            {
                Debug.LogWarning("No tiles found. Generate a map first.");
                return;
            }

            // Test pathfinding between two random walkable tiles
            var walkableTiles = new System.Collections.Generic.List<HexCoordinates>();
            foreach (var kvp in tileMap.Tiles)
            {
                if (kvp.Value.IsWalkable)
                    walkableTiles.Add(kvp.Key);
            }

            if (walkableTiles.Count >= 2)
            {
                var start = walkableTiles[0];
                var end = walkableTiles[walkableTiles.Count - 1];
                
                var path = HexPathfinding.FindPath(start, end, tileMap);
                
                if (path != null)
                {
                    Debug.Log($"Pathfinding test successful! Path length: {path.Count}");
                    foreach (var coord in path)
                    {
                        Debug.Log($"Path step: {coord}");
                    }
                }
                else
                {
                    Debug.Log("No path found between test points.");
                }
            }
        }
    }
}