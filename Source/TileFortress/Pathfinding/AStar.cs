using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TileFortress.GameWorld;

namespace TileFortress.Pathfinding
{
    public static class AStar
    {
        /* f = g + h
         * G is the distance between the current node and the start node.
         * H is the heuristic — estimated distance from the current node to the end node.
         * F is the total cost of the node.
         */

        public static Queue<Point> _frontier;
        public static Dictionary<Point, bool> _visited;

        public static void BreadthFirstSearch(World world, Point start)
        {
            var frontier = new Queue<Point>();
            frontier.Enqueue(start);

            var visited = new Dictionary<Point, bool>();

            _frontier = frontier;
            _visited = visited;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();
                for (int y = -1; y < 2; y++)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        var tilePos = new Point(current.X + x, current.Y + y);
                        var chunkPos = ChunkPosition.FromTilePos(tilePos);
                        if (world.TryGetChunk(chunkPos, out Chunk chunk))
                        {
                            if (!visited.ContainsKey(tilePos))
                            {
                                frontier.Enqueue(tilePos);
                                visited[tilePos] = true;
                            }
                        }
                    }
                }
            }
        }

    }
}