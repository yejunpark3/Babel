using System.Collections.Generic;
using UnityEngine;

namespace HexTileMap
{
    /// <summary>
    /// Main manager for the hexagonal tile map system
    /// </summary>
    public class HexTileMapManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private float hexSize = 1.0f;
        [SerializeField] private Vector2Int mapSize = new Vector2Int(10, 10);
        [SerializeField] private bool generateOnStart = true;

        [Header("Visual Settings")]
        [SerializeField] private GameObject hexTilePrefab;
        [SerializeField] private Material hexTileMaterial;
        [SerializeField] private bool showCoordinates = false;

        [Header("Debug")]
        [SerializeField] private bool showDebugLines = true;
        [SerializeField] private Color debugLineColor = Color.white;

        // Tile storage
        private Dictionary<HexCoordinates, HexTile> tiles = new Dictionary<HexCoordinates, HexTile>();
        private Dictionary<HexCoordinates, GameObject> tileGameObjects = new Dictionary<HexCoordinates, GameObject>();

        public float HexSize => hexSize;
        public Vector2Int MapSize => mapSize;
        public IReadOnlyDictionary<HexCoordinates, HexTile> Tiles => tiles;

        void Start()
        {
            if (generateOnStart)
            {
                GenerateMap();
            }
        }

        /// <summary>
        /// Generates a new hexagonal map
        /// </summary>
        public void GenerateMap()
        {
            ClearMap();
            
            // Generate tiles in rectangular bounds
            for (int q = -mapSize.x / 2; q <= mapSize.x / 2; q++)
            {
                for (int r = -mapSize.y / 2; r <= mapSize.y / 2; r++)
                {
                    var coords = new HexCoordinates(q, r);
                    CreateTile(coords, HexTileType.Grass);
                }
            }

            Debug.Log($"Generated hex map with {tiles.Count} tiles");
        }

        /// <summary>
        /// Creates a tile at the specified coordinates
        /// </summary>
        public HexTile CreateTile(HexCoordinates coords, HexTileType tileType = HexTileType.Empty)
        {
            // Remove existing tile if present
            if (tiles.ContainsKey(coords))
            {
                RemoveTile(coords);
            }

            // Create new tile data
            var tile = new HexTile(coords, tileType);
            tiles[coords] = tile;

            // Create visual representation
            CreateTileGameObject(tile);

            return tile;
        }

        /// <summary>
        /// Removes a tile at the specified coordinates
        /// </summary>
        public bool RemoveTile(HexCoordinates coords)
        {
            if (!tiles.ContainsKey(coords))
                return false;

            // Remove visual representation
            if (tileGameObjects.ContainsKey(coords))
            {
                DestroyImmediate(tileGameObjects[coords]);
                tileGameObjects.Remove(coords);
            }

            // Remove tile data
            tiles.Remove(coords);
            return true;
        }

        /// <summary>
        /// Gets a tile at the specified coordinates
        /// </summary>
        public HexTile GetTile(HexCoordinates coords)
        {
            tiles.TryGetValue(coords, out HexTile tile);
            return tile;
        }

        /// <summary>
        /// Sets the type of a tile at the specified coordinates
        /// </summary>
        public bool SetTileType(HexCoordinates coords, HexTileType tileType)
        {
            var tile = GetTile(coords);
            if (tile == null)
                return false;

            tile.SetTileType(tileType);
            UpdateTileVisual(coords);
            return true;
        }

        /// <summary>
        /// Gets hex coordinates from world position
        /// </summary>
        public HexCoordinates WorldToHex(Vector3 worldPos)
        {
            return HexCoordinates.FromWorldPosition(worldPos, hexSize);
        }

        /// <summary>
        /// Gets world position from hex coordinates
        /// </summary>
        public Vector3 HexToWorld(HexCoordinates coords)
        {
            return coords.ToWorldPosition(hexSize);
        }

        /// <summary>
        /// Gets all neighbors of a tile
        /// </summary>
        public List<HexTile> GetNeighbors(HexCoordinates coords)
        {
            var neighbors = new List<HexTile>();
            var neighborCoords = coords.GetNeighbors();

            foreach (var neighborCoord in neighborCoords)
            {
                var neighbor = GetTile(neighborCoord);
                if (neighbor != null)
                    neighbors.Add(neighbor);
            }

            return neighbors;
        }

        /// <summary>
        /// Clears all tiles from the map
        /// </summary>
        public void ClearMap()
        {
            // Destroy all tile GameObjects
            foreach (var tileGO in tileGameObjects.Values)
            {
                if (tileGO != null)
                    DestroyImmediate(tileGO);
            }

            tiles.Clear();
            tileGameObjects.Clear();
        }

        /// <summary>
        /// Creates a GameObject for a tile
        /// </summary>
        private void CreateTileGameObject(HexTile tile)
        {
            GameObject tileGO;

            if (hexTilePrefab != null)
            {
                tileGO = Instantiate(hexTilePrefab, transform);
            }
            else
            {
                // Create a basic hexagon mesh if no prefab is provided
                tileGO = CreateBasicHexagon();
            }

            tileGO.name = $"HexTile_{tile.Coordinates}";
            tileGO.transform.position = tile.GetWorldPosition(hexSize);

            // Apply tile color
            var renderer = tileGO.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (hexTileMaterial != null)
                {
                    var material = new Material(hexTileMaterial);
                    material.color = tile.TileColor;
                    renderer.material = material;
                }
                else
                {
                    renderer.material.color = tile.TileColor;
                }
            }

            // Add coordinate text if enabled
            if (showCoordinates)
            {
                AddCoordinateText(tileGO, tile.Coordinates);
            }

            tileGameObjects[tile.Coordinates] = tileGO;
        }

        /// <summary>
        /// Creates a basic hexagon mesh
        /// </summary>
        private GameObject CreateBasicHexagon()
        {
            var go = new GameObject();
            var meshFilter = go.AddComponent<MeshFilter>();
            var meshRenderer = go.AddComponent<MeshRenderer>();

            // Create hexagon mesh
            var mesh = CreateHexagonMesh();
            meshFilter.mesh = mesh;

            // Create basic material if none provided
            if (hexTileMaterial != null)
            {
                meshRenderer.material = new Material(hexTileMaterial);
            }
            else
            {
                meshRenderer.material = new Material(Shader.Find("Standard"));
            }

            return go;
        }

        /// <summary>
        /// Creates a hexagon mesh
        /// </summary>
        private Mesh CreateHexagonMesh()
        {
            var mesh = new Mesh();
            mesh.name = "Hexagon";

            // Hexagon vertices (6 corners + center)
            var vertices = new Vector3[7];
            vertices[0] = Vector3.zero; // Center

            for (int i = 0; i < 6; i++)
            {
                float angle = 60f * i * Mathf.Deg2Rad;
                vertices[i + 1] = new Vector3(
                    hexSize * Mathf.Cos(angle),
                    0f,
                    hexSize * Mathf.Sin(angle)
                );
            }

            // Triangles (6 triangles from center to each edge)
            var triangles = new int[18];
            for (int i = 0; i < 6; i++)
            {
                triangles[i * 3] = 0; // Center
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % 6 + 1;
            }

            // UV coordinates
            var uvs = new Vector2[7];
            uvs[0] = new Vector2(0.5f, 0.5f); // Center
            for (int i = 0; i < 6; i++)
            {
                float angle = 60f * i * Mathf.Deg2Rad;
                uvs[i + 1] = new Vector2(
                    0.5f + 0.5f * Mathf.Cos(angle),
                    0.5f + 0.5f * Mathf.Sin(angle)
                );
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();

            return mesh;
        }

        /// <summary>
        /// Updates the visual representation of a tile
        /// </summary>
        private void UpdateTileVisual(HexCoordinates coords)
        {
            if (!tileGameObjects.ContainsKey(coords))
                return;

            var tile = GetTile(coords);
            if (tile == null)
                return;

            var tileGO = tileGameObjects[coords];
            var renderer = tileGO.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = tile.TileColor;
            }
        }

        /// <summary>
        /// Adds coordinate text to a tile GameObject
        /// </summary>
        private void AddCoordinateText(GameObject tileGO, HexCoordinates coords)
        {
            var textGO = new GameObject("CoordinateText");
            textGO.transform.SetParent(tileGO.transform);
            textGO.transform.localPosition = Vector3.up * 0.1f;
            textGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var textMesh = textGO.AddComponent<TextMesh>();
            textMesh.text = $"{coords.Q},{coords.R}";
            textMesh.fontSize = 10;
            textMesh.color = Color.black;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
        }

        /// <summary>
        /// Debug drawing
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showDebugLines)
                return;

            Gizmos.color = debugLineColor;

            foreach (var tile in tiles.Values)
            {
                var center = tile.GetWorldPosition(hexSize);
                var neighbors = tile.Coordinates.GetNeighbors();

                foreach (var neighborCoord in neighbors)
                {
                    var neighborPos = neighborCoord.ToWorldPosition(hexSize);
                    Gizmos.DrawLine(center, neighborPos);
                }
            }
        }

        /// <summary>
        /// Editor utility methods
        /// </summary>
        [ContextMenu("Generate Map")]
        public void GenerateMapEditor()
        {
            GenerateMap();
        }

        [ContextMenu("Clear Map")]
        public void ClearMapEditor()
        {
            ClearMap();
        }
    }
}