using UnityEngine;

namespace HexTileMap
{
    /// <summary>
    /// Represents coordinates in a hexagonal grid using axial coordinate system (q, r)
    /// This system is more efficient than cubic coordinates for most operations
    /// </summary>
    [System.Serializable]
    public struct HexCoordinates
    {
        [SerializeField] private int q; // Column (axial coordinate)
        [SerializeField] private int r; // Row (axial coordinate)

        public int Q => q;
        public int R => r;
        public int S => -q - r; // Third coordinate in cubic system (q + r + s = 0)

        public HexCoordinates(int q, int r)
        {
            this.q = q;
            this.r = r;
        }

        /// <summary>
        /// Creates hex coordinates from cubic coordinates
        /// </summary>
        public static HexCoordinates FromCubic(int q, int r, int s)
        {
            if (q + r + s != 0)
                throw new System.ArgumentException("Invalid cubic coordinates: q + r + s must equal 0");
            return new HexCoordinates(q, r);
        }

        /// <summary>
        /// Converts hex coordinates to world position
        /// </summary>
        public Vector3 ToWorldPosition(float hexSize)
        {
            float x = hexSize * (3f / 2f * q);
            float z = hexSize * (Mathf.Sqrt(3f) / 2f * q + Mathf.Sqrt(3f) * r);
            return new Vector3(x, 0f, z);
        }

        /// <summary>
        /// Converts world position to hex coordinates
        /// </summary>
        public static HexCoordinates FromWorldPosition(Vector3 worldPos, float hexSize)
        {
            float q = (2f / 3f * worldPos.x) / hexSize;
            float r = (-1f / 3f * worldPos.x + Mathf.Sqrt(3f) / 3f * worldPos.z) / hexSize;
            return HexRound(q, r);
        }

        /// <summary>
        /// Rounds fractional hex coordinates to the nearest hex
        /// </summary>
        private static HexCoordinates HexRound(float q, float r)
        {
            float s = -q - r;
            
            int rq = Mathf.RoundToInt(q);
            int rr = Mathf.RoundToInt(r);
            int rs = Mathf.RoundToInt(s);

            float q_diff = Mathf.Abs(rq - q);
            float r_diff = Mathf.Abs(rr - r);
            float s_diff = Mathf.Abs(rs - s);

            if (q_diff > r_diff && q_diff > s_diff)
                rq = -rr - rs;
            else if (r_diff > s_diff)
                rr = -rq - rs;

            return new HexCoordinates(rq, rr);
        }

        /// <summary>
        /// Gets distance between two hex coordinates
        /// </summary>
        public int DistanceTo(HexCoordinates other)
        {
            return (Mathf.Abs(q - other.q) + Mathf.Abs(q + r - other.q - other.r) + Mathf.Abs(r - other.r)) / 2;
        }

        /// <summary>
        /// Gets all neighboring hex coordinates
        /// </summary>
        public HexCoordinates[] GetNeighbors()
        {
            return new HexCoordinates[]
            {
                new HexCoordinates(q + 1, r),     // Right
                new HexCoordinates(q + 1, r - 1), // Top-right
                new HexCoordinates(q, r - 1),     // Top-left
                new HexCoordinates(q - 1, r),     // Left
                new HexCoordinates(q - 1, r + 1), // Bottom-left
                new HexCoordinates(q, r + 1)      // Bottom-right
            };
        }

        /// <summary>
        /// Gets neighbor in specific direction (0-5)
        /// </summary>
        public HexCoordinates GetNeighbor(int direction)
        {
            var neighbors = GetNeighbors();
            return neighbors[direction % 6];
        }

        public override bool Equals(object obj)
        {
            if (obj is HexCoordinates other)
                return q == other.q && r == other.r;
            return false;
        }

        public override int GetHashCode()
        {
            return q.GetHashCode() ^ (r.GetHashCode() << 2);
        }

        public override string ToString()
        {
            return $"Hex({q}, {r})";
        }

        public static bool operator ==(HexCoordinates a, HexCoordinates b)
        {
            return a.q == b.q && a.r == b.r;
        }

        public static bool operator !=(HexCoordinates a, HexCoordinates b)
        {
            return !(a == b);
        }

        public static HexCoordinates operator +(HexCoordinates a, HexCoordinates b)
        {
            return new HexCoordinates(a.q + b.q, a.r + b.r);
        }

        public static HexCoordinates operator -(HexCoordinates a, HexCoordinates b)
        {
            return new HexCoordinates(a.q - b.q, a.r - b.r);
        }
    }
}