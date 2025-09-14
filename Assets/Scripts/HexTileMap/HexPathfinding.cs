using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HexTileMap
{
    /// <summary>
    /// A* pathfinding implementation for hexagonal grids
    /// </summary>
    public static class HexPathfinding
    {
        /// <summary>
        /// Node used for A* pathfinding
        /// </summary>
        private class PathNode
        {
            public HexCoordinates coordinates;
            public PathNode parent;
            public float gCost; // Distance from start
            public float hCost; // Heuristic distance to end
            public float FCost => gCost + hCost;

            public PathNode(HexCoordinates coords)
            {
                coordinates = coords;
            }
        }

        /// <summary>
        /// Finds a path between two hexagonal coordinates using A*
        /// </summary>
        /// <param name="start">Starting hex coordinates</param>
        /// <param name="end">Target hex coordinates</param>
        /// <param name="tileMap">The hex tile map to search in</param>
        /// <param name="allowDiagonals">Whether to allow diagonal movement</param>
        /// <returns>List of coordinates representing the path, or null if no path found</returns>
        public static List<HexCoordinates> FindPath(HexCoordinates start, HexCoordinates end, HexTileMapManager tileMap, bool allowDiagonals = true)
        {
            var startTile = tileMap.GetTile(start);
            var endTile = tileMap.GetTile(end);

            if (startTile == null || endTile == null || !endTile.IsWalkable)
                return null;

            var openSet = new List<PathNode>();
            var closedSet = new HashSet<HexCoordinates>();

            var startNode = new PathNode(start);
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                // Find node with lowest F cost
                var currentNode = openSet.OrderBy(node => node.FCost).ThenBy(node => node.hCost).First();
                openSet.Remove(currentNode);
                closedSet.Add(currentNode.coordinates);

                // Check if we reached the target
                if (currentNode.coordinates == end)
                {
                    return RetracePath(startNode, currentNode);
                }

                // Check all neighbors
                var neighbors = currentNode.coordinates.GetNeighbors();
                foreach (var neighborCoord in neighbors)
                {
                    if (closedSet.Contains(neighborCoord))
                        continue;

                    var neighborTile = tileMap.GetTile(neighborCoord);
                    if (neighborTile == null || !neighborTile.IsWalkable)
                        continue;

                    float newCostToNeighbor = currentNode.gCost + neighborTile.MovementCost;

                    var neighborNode = openSet.FirstOrDefault(node => node.coordinates == neighborCoord);
                    if (neighborNode == null)
                    {
                        neighborNode = new PathNode(neighborCoord);
                        neighborNode.parent = currentNode;
                        neighborNode.gCost = newCostToNeighbor;
                        neighborNode.hCost = GetHeuristic(neighborCoord, end);
                        openSet.Add(neighborNode);
                    }
                    else if (newCostToNeighbor < neighborNode.gCost)
                    {
                        neighborNode.parent = currentNode;
                        neighborNode.gCost = newCostToNeighbor;
                    }
                }
            }

            return null; // No path found
        }

        /// <summary>
        /// Retraces the path from end node to start node
        /// </summary>
        private static List<HexCoordinates> RetracePath(PathNode startNode, PathNode endNode)
        {
            var path = new List<HexCoordinates>();
            var currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode.coordinates);
                currentNode = currentNode.parent;
            }

            path.Add(startNode.coordinates);
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Calculates heuristic distance between two hex coordinates
        /// </summary>
        private static float GetHeuristic(HexCoordinates from, HexCoordinates to)
        {
            return from.DistanceTo(to);
        }

        /// <summary>
        /// Gets all tiles within a certain range
        /// </summary>
        public static List<HexCoordinates> GetTilesInRange(HexCoordinates center, int range)
        {
            var tilesInRange = new List<HexCoordinates>();

            for (int q = -range; q <= range; q++)
            {
                int r1 = Mathf.Max(-range, -q - range);
                int r2 = Mathf.Min(range, -q + range);

                for (int r = r1; r <= r2; r++)
                {
                    tilesInRange.Add(new HexCoordinates(center.Q + q, center.R + r));
                }
            }

            return tilesInRange;
        }

        /// <summary>
        /// Gets all tiles along a line between two coordinates
        /// </summary>
        public static List<HexCoordinates> GetLine(HexCoordinates start, HexCoordinates end)
        {
            int distance = start.DistanceTo(end);
            if (distance == 0)
                return new List<HexCoordinates> { start };

            var results = new List<HexCoordinates>();

            for (int i = 0; i <= distance; i++)
            {
                float t = (float)i / distance;
                var coords = HexLerp(start, end, t);
                results.Add(coords);
            }

            return results;
        }

        /// <summary>
        /// Linear interpolation between two hex coordinates
        /// </summary>
        private static HexCoordinates HexLerp(HexCoordinates a, HexCoordinates b, float t)
        {
            float q = Mathf.Lerp(a.Q, b.Q, t);
            float r = Mathf.Lerp(a.R, b.R, t);
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
        /// Gets all reachable tiles within movement range
        /// </summary>
        public static List<HexCoordinates> GetReachableTiles(HexCoordinates start, int movement, HexTileMapManager tileMap)
        {
            var reachable = new List<HexCoordinates>();
            var visited = new HashSet<HexCoordinates>();
            var fringes = new List<List<HexCoordinates>>();

            fringes.Add(new List<HexCoordinates> { start });
            visited.Add(start);

            for (int k = 1; k <= movement; k++)
            {
                fringes.Add(new List<HexCoordinates>());
                
                foreach (var coord in fringes[k - 1])
                {
                    var neighbors = coord.GetNeighbors();
                    foreach (var neighbor in neighbors)
                    {
                        if (visited.Contains(neighbor))
                            continue;

                        var tile = tileMap.GetTile(neighbor);
                        if (tile == null || !tile.IsWalkable || tile.MovementCost > movement)
                            continue;

                        visited.Add(neighbor);
                        fringes[k].Add(neighbor);
                        reachable.Add(neighbor);
                    }
                }
            }

            return reachable;
        }
    }
}