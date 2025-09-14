using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace HexTileMap
{
    /// <summary>
    /// Handles mouse interaction with the hex tile map
    /// </summary>
    public class HexTileMapInteraction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HexTileMapManager tileMapManager;
        [SerializeField] private Camera playerCamera;

        [Header("Selection Settings")]
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Material selectedMaterial;
        [SerializeField] private GameObject selectionIndicator;

        [Header("Tile Painting")]
        [SerializeField] private HexTileType paintTileType = HexTileType.Grass;
        [SerializeField] private bool paintModeEnabled = false;

        [Header("Pathfinding Demo")]
        [SerializeField] private bool showPathfinding = true;
        [SerializeField] private Material pathMaterial;
        [SerializeField] private LineRenderer pathLineRenderer;

        // State
        private HexCoordinates? hoveredTile;
        private HexCoordinates? selectedTile;
        private HexCoordinates? pathStart;
        private HexCoordinates? pathEnd;
        private List<HexCoordinates> currentPath;
        private Dictionary<HexCoordinates, Material> originalMaterials = new Dictionary<HexCoordinates, Material>();

        // Events
        public System.Action<HexCoordinates> OnTileHovered;
        public System.Action<HexCoordinates> OnTileSelected;
        public System.Action<HexCoordinates, HexTileType> OnTilePainted;

        private void Start()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;

            if (tileMapManager == null)
                tileMapManager = FindObjectOfType<HexTileMapManager>();

            SetupPathLineRenderer();
        }

        private void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            HandleMouseInput();
        }

        /// <summary>
        /// Handles mouse input for tile interaction
        /// </summary>
        private void HandleMouseInput()
        {
            var mousePosition = Input.mousePosition;
            var ray = playerCamera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var worldPos = hit.point;
                var hexCoords = tileMapManager.WorldToHex(worldPos);
                var tile = tileMapManager.GetTile(hexCoords);

                if (tile != null)
                {
                    HandleTileHover(hexCoords);

                    if (Input.GetMouseButtonDown(0)) // Left click
                    {
                        HandleTileClick(hexCoords);
                    }
                    else if (Input.GetMouseButtonDown(1)) // Right click
                    {
                        HandleTileRightClick(hexCoords);
                    }

                    if (paintModeEnabled && Input.GetMouseButton(0))
                    {
                        PaintTile(hexCoords);
                    }
                }
            }
            else
            {
                ClearHover();
            }

            // Handle keyboard input
            HandleKeyboardInput();
        }

        /// <summary>
        /// Handles tile hover
        /// </summary>
        private void HandleTileHover(HexCoordinates coords)
        {
            if (hoveredTile != coords)
            {
                ClearHover();
                hoveredTile = coords;
                HighlightTile(coords, highlightMaterial);
                OnTileHovered?.Invoke(coords);
            }
        }

        /// <summary>
        /// Handles tile click (selection)
        /// </summary>
        private void HandleTileClick(HexCoordinates coords)
        {
            if (showPathfinding)
            {
                if (pathStart == null)
                {
                    pathStart = coords;
                    SelectTile(coords);
                }
                else if (pathEnd == null && coords != pathStart)
                {
                    pathEnd = coords;
                    CalculateAndShowPath();
                }
                else
                {
                    // Reset pathfinding
                    ClearPath();
                    pathStart = coords;
                    pathEnd = null;
                    SelectTile(coords);
                }
            }
            else
            {
                SelectTile(coords);
            }

            OnTileSelected?.Invoke(coords);
        }

        /// <summary>
        /// Handles right click (context actions)
        /// </summary>
        private void HandleTileRightClick(HexCoordinates coords)
        {
            if (paintModeEnabled)
            {
                // Cycle through tile types
                var tile = tileMapManager.GetTile(coords);
                if (tile != null)
                {
                    var currentType = tile.TileType;
                    var nextType = GetNextTileType(currentType);
                    PaintTile(coords, nextType);
                }
            }
        }

        /// <summary>
        /// Handles keyboard input
        /// </summary>
        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                paintModeEnabled = !paintModeEnabled;
                Debug.Log($"Paint mode: {(paintModeEnabled ? "ON" : "OFF")}");
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                ClearSelection();
                ClearPath();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (selectedTile.HasValue)
                {
                    ShowReachableTiles(selectedTile.Value, 3);
                }
            }

            // Number keys for tile types
            for (int i = 1; i <= 6; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    paintTileType = (HexTileType)i;
                    Debug.Log($"Paint tile type set to: {paintTileType}");
                }
            }
        }

        /// <summary>
        /// Selects a tile
        /// </summary>
        private void SelectTile(HexCoordinates coords)
        {
            ClearSelection();
            selectedTile = coords;
            HighlightTile(coords, selectedMaterial);

            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(true);
                selectionIndicator.transform.position = tileMapManager.HexToWorld(coords) + Vector3.up * 0.1f;
            }
        }

        /// <summary>
        /// Clears tile selection
        /// </summary>
        private void ClearSelection()
        {
            if (selectedTile.HasValue)
            {
                RestoreOriginalMaterial(selectedTile.Value);
                selectedTile = null;
            }

            if (selectionIndicator != null)
                selectionIndicator.SetActive(false);
        }

        /// <summary>
        /// Clears tile hover
        /// </summary>
        private void ClearHover()
        {
            if (hoveredTile.HasValue && hoveredTile != selectedTile)
            {
                RestoreOriginalMaterial(hoveredTile.Value);
                hoveredTile = null;
            }
        }

        /// <summary>
        /// Highlights a tile with the given material
        /// </summary>
        private void HighlightTile(HexCoordinates coords, Material material)
        {
            if (material == null)
                return;

            var tileGO = GetTileGameObject(coords);
            if (tileGO != null)
            {
                var renderer = tileGO.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (!originalMaterials.ContainsKey(coords))
                    {
                        originalMaterials[coords] = renderer.material;
                    }
                    renderer.material = material;
                }
            }
        }

        /// <summary>
        /// Restores original material for a tile
        /// </summary>
        private void RestoreOriginalMaterial(HexCoordinates coords)
        {
            if (originalMaterials.ContainsKey(coords))
            {
                var tileGO = GetTileGameObject(coords);
                if (tileGO != null)
                {
                    var renderer = tileGO.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = originalMaterials[coords];
                    }
                }
                originalMaterials.Remove(coords);
            }
        }

        /// <summary>
        /// Paints a tile with the current paint type
        /// </summary>
        private void PaintTile(HexCoordinates coords, HexTileType? tileType = null)
        {
            var type = tileType ?? paintTileType;
            if (tileMapManager.SetTileType(coords, type))
            {
                OnTilePainted?.Invoke(coords, type);
            }
        }

        /// <summary>
        /// Gets the next tile type in sequence
        /// </summary>
        private HexTileType GetNextTileType(HexTileType currentType)
        {
            var types = System.Enum.GetValues(typeof(HexTileType));
            int currentIndex = System.Array.IndexOf(types, currentType);
            int nextIndex = (currentIndex + 1) % types.Length;
            return (HexTileType)types.GetValue(nextIndex);
        }

        /// <summary>
        /// Calculates and shows path between start and end
        /// </summary>
        private void CalculateAndShowPath()
        {
            if (!pathStart.HasValue || !pathEnd.HasValue)
                return;

            currentPath = HexPathfinding.FindPath(pathStart.Value, pathEnd.Value, tileMapManager);
            
            if (currentPath != null)
            {
                Debug.Log($"Path found with {currentPath.Count} steps");
                DrawPathLine();
            }
            else
            {
                Debug.Log("No path found");
                ClearPath();
            }
        }

        /// <summary>
        /// Shows tiles reachable within movement range
        /// </summary>
        private void ShowReachableTiles(HexCoordinates start, int movement)
        {
            var reachable = HexPathfinding.GetReachableTiles(start, movement, tileMapManager);
            
            foreach (var coords in reachable)
            {
                HighlightTile(coords, pathMaterial);
            }

            Debug.Log($"Found {reachable.Count} reachable tiles within {movement} movement");
        }

        /// <summary>
        /// Clears the current path
        /// </summary>
        private void ClearPath()
        {
            pathStart = null;
            pathEnd = null;
            currentPath = null;

            if (pathLineRenderer != null)
                pathLineRenderer.positionCount = 0;
        }

        /// <summary>
        /// Sets up the path line renderer
        /// </summary>
        private void SetupPathLineRenderer()
        {
            if (pathLineRenderer == null)
            {
                var pathGO = new GameObject("PathLine");
                pathGO.transform.SetParent(transform);
                pathLineRenderer = pathGO.AddComponent<LineRenderer>();
                pathLineRenderer.material = pathMaterial ?? new Material(Shader.Find("Sprites/Default"));
                pathLineRenderer.color = Color.red;
                pathLineRenderer.width = 0.1f;
                pathLineRenderer.useWorldSpace = true;
            }
        }

        /// <summary>
        /// Draws the path line
        /// </summary>
        private void DrawPathLine()
        {
            if (pathLineRenderer == null || currentPath == null)
                return;

            pathLineRenderer.positionCount = currentPath.Count;
            
            for (int i = 0; i < currentPath.Count; i++)
            {
                var worldPos = tileMapManager.HexToWorld(currentPath[i]);
                worldPos.y += 0.1f; // Raise slightly above tiles
                pathLineRenderer.SetPosition(i, worldPos);
            }
        }

        /// <summary>
        /// Gets the GameObject for a tile
        /// </summary>
        private GameObject GetTileGameObject(HexCoordinates coords)
        {
            // This assumes the tile GameObjects are stored in the tile map manager
            // In a real implementation, you might want to expose this through the manager
            return null; // TODO: Implement based on HexTileMapManager structure
        }

        /// <summary>
        /// Gets information about the currently selected tile
        /// </summary>
        public string GetSelectedTileInfo()
        {
            if (!selectedTile.HasValue)
                return "No tile selected";

            var tile = tileMapManager.GetTile(selectedTile.Value);
            if (tile == null)
                return "Invalid tile";

            return $"Tile: {tile.Coordinates}\nType: {tile.TileType}\nWalkable: {tile.IsWalkable}\nCost: {tile.MovementCost}";
        }

        /// <summary>
        /// Toggle paint mode
        /// </summary>
        public void TogglePaintMode()
        {
            paintModeEnabled = !paintModeEnabled;
        }

        /// <summary>
        /// Set paint tile type
        /// </summary>
        public void SetPaintTileType(HexTileType tileType)
        {
            paintTileType = tileType;
        }
    }
}