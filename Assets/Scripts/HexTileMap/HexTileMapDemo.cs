using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace HexTileMap
{
    /// <summary>
    /// Demo controller showcasing the hexagonal tile map system features
    /// </summary>
    public class HexTileMapDemo : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HexTileMapManager tileMapManager;
        [SerializeField] private HexTileMapInteraction interaction;

        [Header("UI Elements")]
        [SerializeField] private Text infoText;
        [SerializeField] private Text instructionsText;
        [SerializeField] private Button generateMapButton;
        [SerializeField] private Button clearMapButton;
        [SerializeField] private Toggle paintModeToggle;
        [SerializeField] private Dropdown tileTypeDropdown;

        [Header("Demo Settings")]
        [SerializeField] private bool autoGenerateOnStart = true;
        [SerializeField] private int demoMapSize = 8;

        private void Start()
        {
            SetupDemo();
            SetupUI();
            
            if (autoGenerateOnStart)
            {
                GenerateDemoMap();
            }
        }

        private void Update()
        {
            UpdateInfoDisplay();
        }

        /// <summary>
        /// Sets up the demo
        /// </summary>
        private void SetupDemo()
        {
            if (tileMapManager == null)
                tileMapManager = FindObjectOfType<HexTileMapManager>();

            if (interaction == null)
                interaction = FindObjectOfType<HexTileMapInteraction>();

            // Subscribe to interaction events
            if (interaction != null)
            {
                interaction.OnTileSelected += OnTileSelected;
                interaction.OnTileHovered += OnTileHovered;
                interaction.OnTilePainted += OnTilePainted;
            }
        }

        /// <summary>
        /// Sets up UI elements
        /// </summary>
        private void SetupUI()
        {
            if (generateMapButton != null)
                generateMapButton.onClick.AddListener(GenerateDemoMap);

            if (clearMapButton != null)
                clearMapButton.onClick.AddListener(ClearMap);

            if (paintModeToggle != null)
            {
                paintModeToggle.onValueChanged.AddListener(OnPaintModeToggled);
            }

            if (tileTypeDropdown != null)
            {
                SetupTileTypeDropdown();
                tileTypeDropdown.onValueChanged.AddListener(OnTileTypeChanged);
            }

            if (instructionsText != null)
            {
                instructionsText.text = GetInstructionsText();
            }
        }

        /// <summary>
        /// Sets up the tile type dropdown
        /// </summary>
        private void SetupTileTypeDropdown()
        {
            var options = new List<Dropdown.OptionData>();
            var tileTypes = System.Enum.GetValues(typeof(HexTileType));

            foreach (HexTileType tileType in tileTypes)
            {
                options.Add(new Dropdown.OptionData(tileType.ToString()));
            }

            tileTypeDropdown.options = options;
            tileTypeDropdown.value = 1; // Default to Grass
        }

        /// <summary>
        /// Generates a demo map with various tile types
        /// </summary>
        public void GenerateDemoMap()
        {
            if (tileMapManager == null)
                return;

            tileMapManager.ClearMap();

            // Generate base grass tiles
            for (int q = -demoMapSize; q <= demoMapSize; q++)
            {
                for (int r = -demoMapSize; r <= demoMapSize; r++)
                {
                    var coords = new HexCoordinates(q, r);
                    tileMapManager.CreateTile(coords, HexTileType.Grass);
                }
            }

            // Add some water tiles (creates a river)
            for (int i = -3; i <= 3; i++)
            {
                tileMapManager.SetTileType(new HexCoordinates(i, -1), HexTileType.Water);
                tileMapManager.SetTileType(new HexCoordinates(i, 0), HexTileType.Water);
            }

            // Add some mountain tiles
            tileMapManager.SetTileType(new HexCoordinates(-4, 2), HexTileType.Mountain);
            tileMapManager.SetTileType(new HexCoordinates(-3, 2), HexTileType.Mountain);
            tileMapManager.SetTileType(new HexCoordinates(-4, 3), HexTileType.Mountain);

            // Add forest area
            for (int q = 2; q <= 4; q++)
            {
                for (int r = 2; r <= 4; r++)
                {
                    tileMapManager.SetTileType(new HexCoordinates(q, r), HexTileType.Forest);
                }
            }

            // Add some desert tiles
            for (int i = -2; i <= 0; i++)
            {
                tileMapManager.SetTileType(new HexCoordinates(i, -4), HexTileType.Desert);
                tileMapManager.SetTileType(new HexCoordinates(i, -3), HexTileType.Desert);
            }

            Debug.Log("Demo map generated!");
        }

        /// <summary>
        /// Clears the map
        /// </summary>
        public void ClearMap()
        {
            if (tileMapManager != null)
            {
                tileMapManager.ClearMap();
                Debug.Log("Map cleared!");
            }
        }

        /// <summary>
        /// Updates the info display
        /// </summary>
        private void UpdateInfoDisplay()
        {
            if (infoText == null)
                return;

            var info = "";
            
            if (tileMapManager != null)
            {
                info += $"총 타일 수: {tileMapManager.Tiles.Count}\n";
                info += $"육각형 크기: {tileMapManager.HexSize:F1}\n";
                info += $"맵 크기: {tileMapManager.MapSize}\n\n";
            }

            if (interaction != null)
            {
                info += interaction.GetSelectedTileInfo();
            }

            infoText.text = info;
        }

        /// <summary>
        /// Gets instructions text
        /// </summary>
        private string GetInstructionsText()
        {
            return "육각형 타일 맵 데모\n\n" +
                   "조작법:\n" +
                   "• 좌클릭: 타일 선택/경로 설정\n" +
                   "• 우클릭: 타일 타입 변경 (페인트 모드에서)\n" +
                   "• P키: 페인트 모드 토글\n" +
                   "• C키: 선택 및 경로 초기화\n" +
                   "• R키: 이동 가능 범위 표시\n" +
                   "• 1-6키: 페인트 타일 타입 변경\n\n" +
                   "기능:\n" +
                   "• 육각형 좌표 시스템\n" +
                   "• A* 경로 찾기\n" +
                   "• 타일 페인팅\n" +
                   "• 이동 범위 계산";
        }

        /// <summary>
        /// Called when a tile is selected
        /// </summary>
        private void OnTileSelected(HexCoordinates coords)
        {
            Debug.Log($"타일 선택됨: {coords}");
        }

        /// <summary>
        /// Called when a tile is hovered
        /// </summary>
        private void OnTileHovered(HexCoordinates coords)
        {
            // Optional: Show hover info
        }

        /// <summary>
        /// Called when a tile is painted
        /// </summary>
        private void OnTilePainted(HexCoordinates coords, HexTileType tileType)
        {
            Debug.Log($"타일 페인팅: {coords} -> {tileType}");
        }

        /// <summary>
        /// Called when paint mode is toggled
        /// </summary>
        private void OnPaintModeToggled(bool enabled)
        {
            if (interaction != null)
            {
                interaction.TogglePaintMode();
            }
        }

        /// <summary>
        /// Called when tile type selection changes
        /// </summary>
        private void OnTileTypeChanged(int index)
        {
            var tileType = (HexTileType)index;
            if (interaction != null)
            {
                interaction.SetPaintTileType(tileType);
            }
        }

        /// <summary>
        /// Demonstrates pathfinding between random tiles
        /// </summary>
        [ContextMenu("Demo Pathfinding")]
        public void DemoPathfinding()
        {
            if (tileMapManager == null || tileMapManager.Tiles.Count == 0)
                return;

            var tiles = new List<HexCoordinates>(tileMapManager.Tiles.Keys);
            var walkableTiles = new List<HexCoordinates>();

            foreach (var coords in tiles)
            {
                var tile = tileMapManager.GetTile(coords);
                if (tile != null && tile.IsWalkable)
                    walkableTiles.Add(coords);
            }

            if (walkableTiles.Count < 2)
                return;

            var start = walkableTiles[Random.Range(0, walkableTiles.Count)];
            var end = walkableTiles[Random.Range(0, walkableTiles.Count)];

            var path = HexPathfinding.FindPath(start, end, tileMapManager);
            
            if (path != null)
            {
                Debug.Log($"경로 찾기 데모: {start} -> {end}, 경로 길이: {path.Count}");
                foreach (var coord in path)
                {
                    Debug.Log($"  경로: {coord}");
                }
            }
            else
            {
                Debug.Log($"경로를 찾을 수 없음: {start} -> {end}");
            }
        }

        /// <summary>
        /// Demonstrates range calculation
        /// </summary>
        [ContextMenu("Demo Range Calculation")]
        public void DemoRangeCalculation()
        {
            var center = new HexCoordinates(0, 0);
            var range = 3;
            var tilesInRange = HexPathfinding.GetTilesInRange(center, range);

            Debug.Log($"범위 {range} 내의 타일 수: {tilesInRange.Count}");
            foreach (var coords in tilesInRange)
            {
                Debug.Log($"  범위 내 타일: {coords}");
            }
        }
    }
}