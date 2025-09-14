using UnityEngine;
using System.Collections.Generic;

namespace HexTileMap.Tests
{
    /// <summary>
    /// Integration test for the hexagonal tile map system
    /// Run this to verify all components work together correctly
    /// </summary>
    public class HexTileMapSystemTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runTestOnStart = true;
        [SerializeField] private bool verbose = true;

        private void Start()
        {
            if (runTestOnStart)
            {
                RunAllTests();
            }
        }

        /// <summary>
        /// Runs all system tests
        /// </summary>
        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            Log("=== 육각형 타일 맵 시스템 테스트 시작 ===");

            bool allTestsPassed = true;

            allTestsPassed &= TestHexCoordinates();
            allTestsPassed &= TestHexTile();
            allTestsPassed &= TestHexTileMapManager();
            allTestsPassed &= TestHexPathfinding();

            Log($"=== 테스트 완료: {(allTestsPassed ? "모든 테스트 통과" : "일부 테스트 실패")} ===");
        }

        /// <summary>
        /// Tests HexCoordinates functionality
        /// </summary>
        private bool TestHexCoordinates()
        {
            Log("HexCoordinates 테스트 중...");

            try
            {
                // Test coordinate creation
                var coord1 = new HexCoordinates(0, 0);
                var coord2 = new HexCoordinates(1, -1);

                // Test distance calculation
                int distance = coord1.DistanceTo(coord2);
                if (distance != 1)
                {
                    LogError($"거리 계산 오류: 예상 1, 실제 {distance}");
                    return false;
                }

                // Test neighbor calculation
                var neighbors = coord1.GetNeighbors();
                if (neighbors.Length != 6)
                {
                    LogError($"이웃 개수 오류: 예상 6, 실제 {neighbors.Length}");
                    return false;
                }

                // Test world position conversion
                var worldPos = coord1.ToWorldPosition(1.0f);
                var backToHex = HexCoordinates.FromWorldPosition(worldPos, 1.0f);
                if (backToHex != coord1)
                {
                    LogError($"좌표 변환 오류: {coord1} -> {worldPos} -> {backToHex}");
                    return false;
                }

                Log("HexCoordinates 테스트 통과");
                return true;
            }
            catch (System.Exception e)
            {
                LogError($"HexCoordinates 테스트 실패: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tests HexTile functionality
        /// </summary>
        private bool TestHexTile()
        {
            Log("HexTile 테스트 중...");

            try
            {
                var coords = new HexCoordinates(0, 0);
                var tile = new HexTile(coords, HexTileType.Grass);

                // Test basic properties
                if (tile.Coordinates != coords)
                {
                    LogError("타일 좌표 불일치");
                    return false;
                }

                if (tile.TileType != HexTileType.Grass)
                {
                    LogError("타일 타입 불일치");
                    return false;
                }

                // Test tile type changing
                tile.SetTileType(HexTileType.Water);
                if (tile.TileType != HexTileType.Water || tile.IsWalkable)
                {
                    LogError("타일 타입 변경 실패");
                    return false;
                }

                Log("HexTile 테스트 통과");
                return true;
            }
            catch (System.Exception e)
            {
                LogError($"HexTile 테스트 실패: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tests HexTileMapManager functionality
        /// </summary>
        private bool TestHexTileMapManager()
        {
            Log("HexTileMapManager 테스트 중...");

            try
            {
                // Create a temporary manager for testing
                var managerGO = new GameObject("TestTileMapManager");
                var manager = managerGO.AddComponent<HexTileMapManager>();

                // Test tile creation
                var coords = new HexCoordinates(0, 0);
                var tile = manager.CreateTile(coords, HexTileType.Grass);

                if (tile == null)
                {
                    LogError("타일 생성 실패");
                    DestroyImmediate(managerGO);
                    return false;
                }

                // Test tile retrieval
                var retrievedTile = manager.GetTile(coords);
                if (retrievedTile != tile)
                {
                    LogError("타일 검색 실패");
                    DestroyImmediate(managerGO);
                    return false;
                }

                // Test coordinate conversion
                var worldPos = manager.HexToWorld(coords);
                var backToHex = manager.WorldToHex(worldPos);
                if (backToHex != coords)
                {
                    LogError($"좌표 변환 실패: {coords} -> {worldPos} -> {backToHex}");
                    DestroyImmediate(managerGO);
                    return false;
                }

                // Test tile removal
                bool removed = manager.RemoveTile(coords);
                if (!removed || manager.GetTile(coords) != null)
                {
                    LogError("타일 제거 실패");
                    DestroyImmediate(managerGO);
                    return false;
                }

                DestroyImmediate(managerGO);
                Log("HexTileMapManager 테스트 통과");
                return true;
            }
            catch (System.Exception e)
            {
                LogError($"HexTileMapManager 테스트 실패: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tests HexPathfinding functionality
        /// </summary>
        private bool TestHexPathfinding()
        {
            Log("HexPathfinding 테스트 중...");

            try
            {
                // Create a test map
                var managerGO = new GameObject("TestPathfindingManager");
                var manager = managerGO.AddComponent<HexTileMapManager>();

                // Create a simple 3x3 map
                for (int q = -1; q <= 1; q++)
                {
                    for (int r = -1; r <= 1; r++)
                    {
                        var coords = new HexCoordinates(q, r);
                        manager.CreateTile(coords, HexTileType.Grass);
                    }
                }

                // Test pathfinding
                var start = new HexCoordinates(-1, -1);
                var end = new HexCoordinates(1, 1);
                var path = HexPathfinding.FindPath(start, end, manager);

                if (path == null)
                {
                    LogError("경로 찾기 실패");
                    DestroyImmediate(managerGO);
                    return false;
                }

                if (path.Count < 2)
                {
                    LogError($"경로 길이 오류: 예상 >= 2, 실제 {path.Count}");
                    DestroyImmediate(managerGO);
                    return false;
                }

                if (path[0] != start || path[path.Count - 1] != end)
                {
                    LogError("경로 시작점 또는 끝점 오류");
                    DestroyImmediate(managerGO);
                    return false;
                }

                // Test range calculation
                var center = new HexCoordinates(0, 0);
                var tilesInRange = HexPathfinding.GetTilesInRange(center, 1);
                if (tilesInRange.Count != 7) // center + 6 neighbors
                {
                    LogError($"범위 계산 오류: 예상 7, 실제 {tilesInRange.Count}");
                    DestroyImmediate(managerGO);
                    return false;
                }

                // Test blocked pathfinding
                manager.SetTileType(new HexCoordinates(0, 0), HexTileType.Water);
                var blockedPath = HexPathfinding.FindPath(start, end, manager);
                if (blockedPath != null && blockedPath.Count <= path.Count)
                {
                    LogError("차단된 경로에서 더 짧은 경로 발견");
                    DestroyImmediate(managerGO);
                    return false;
                }

                DestroyImmediate(managerGO);
                Log("HexPathfinding 테스트 통과");
                return true;
            }
            catch (System.Exception e)
            {
                LogError($"HexPathfinding 테스트 실패: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Performance test for large maps
        /// </summary>
        [ContextMenu("Performance Test")]
        public void PerformanceTest()
        {
            Log("=== 성능 테스트 시작 ===");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Create large map
            var managerGO = new GameObject("PerformanceTestManager");
            var manager = managerGO.AddComponent<HexTileMapManager>();

            var mapSize = 50;
            for (int q = -mapSize; q <= mapSize; q++)
            {
                for (int r = -mapSize; r <= mapSize; r++)
                {
                    var coords = new HexCoordinates(q, r);
                    manager.CreateTile(coords, HexTileType.Grass);
                }
            }

            stopwatch.Stop();
            var creationTime = stopwatch.ElapsedMilliseconds;
            Log($"대형 맵 생성 ({manager.Tiles.Count} 타일): {creationTime}ms");

            // Test pathfinding performance
            stopwatch.Restart();
            var start = new HexCoordinates(-mapSize, -mapSize);
            var end = new HexCoordinates(mapSize, mapSize);
            var path = HexPathfinding.FindPath(start, end, manager);
            stopwatch.Stop();

            var pathfindingTime = stopwatch.ElapsedMilliseconds;
            Log($"대형 맵 경로 찾기 (거리 {start.DistanceTo(end)}): {pathfindingTime}ms");
            if (path != null)
                Log($"경로 길이: {path.Count}");

            DestroyImmediate(managerGO);
            Log("=== 성능 테스트 완료 ===");
        }

        private void Log(string message)
        {
            if (verbose)
                Debug.Log($"[HexTileMapTest] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[HexTileMapTest] {message}");
        }
    }
}