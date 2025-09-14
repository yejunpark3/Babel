using UnityEngine;

namespace HexTileMap
{
    /// <summary>
    /// Defines the type of hex tile
    /// </summary>
    public enum HexTileType
    {
        Empty = 0,
        Grass = 1,
        Water = 2,
        Mountain = 3,
        Forest = 4,
        Desert = 5,
        Stone = 6
    }

    /// <summary>
    /// Represents a single hexagonal tile with its properties
    /// </summary>
    [System.Serializable]
    public class HexTile
    {
        [SerializeField] private HexCoordinates coordinates;
        [SerializeField] private HexTileType tileType;
        [SerializeField] private bool isWalkable;
        [SerializeField] private int movementCost;
        [SerializeField] private Color tileColor;

        public HexCoordinates Coordinates => coordinates;
        public HexTileType TileType => tileType;
        public bool IsWalkable => isWalkable;
        public int MovementCost => movementCost;
        public Color TileColor => tileColor;

        public HexTile(HexCoordinates coords, HexTileType type = HexTileType.Empty)
        {
            coordinates = coords;
            SetTileType(type);
        }

        /// <summary>
        /// Sets the tile type and updates related properties
        /// </summary>
        public void SetTileType(HexTileType type)
        {
            tileType = type;
            UpdateTileProperties();
        }

        /// <summary>
        /// Updates tile properties based on type
        /// </summary>
        private void UpdateTileProperties()
        {
            switch (tileType)
            {
                case HexTileType.Empty:
                    isWalkable = true;
                    movementCost = 1;
                    tileColor = Color.white;
                    break;
                case HexTileType.Grass:
                    isWalkable = true;
                    movementCost = 1;
                    tileColor = Color.green;
                    break;
                case HexTileType.Water:
                    isWalkable = false;
                    movementCost = int.MaxValue;
                    tileColor = Color.blue;
                    break;
                case HexTileType.Mountain:
                    isWalkable = false;
                    movementCost = int.MaxValue;
                    tileColor = Color.gray;
                    break;
                case HexTileType.Forest:
                    isWalkable = true;
                    movementCost = 2;
                    tileColor = new Color(0f, 0.5f, 0f); // Dark green
                    break;
                case HexTileType.Desert:
                    isWalkable = true;
                    movementCost = 2;
                    tileColor = Color.yellow;
                    break;
                case HexTileType.Stone:
                    isWalkable = true;
                    movementCost = 1;
                    tileColor = new Color(0.7f, 0.7f, 0.7f); // Light gray
                    break;
            }
        }

        /// <summary>
        /// Gets world position of this tile
        /// </summary>
        public Vector3 GetWorldPosition(float hexSize)
        {
            return coordinates.ToWorldPosition(hexSize);
        }

        /// <summary>
        /// Gets distance to another tile
        /// </summary>
        public int GetDistanceTo(HexTile other)
        {
            return coordinates.DistanceTo(other.coordinates);
        }

        public override string ToString()
        {
            return $"HexTile({coordinates}, {tileType})";
        }
    }
}