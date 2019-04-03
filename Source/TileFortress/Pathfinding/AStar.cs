using System.Collections.Generic;
using System.Threading;
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

        public static Queue<TilePosition> _frontier;
        public static Dictionary<TilePosition, bool> _visited;
        public static bool Continue = true;

        public static void BreadthFirstSearch(World world, TilePosition start, TilePosition end)
        {
            var frontier = new Queue<TilePosition>();
            var visited = new Dictionary<TilePosition, bool>();

            // drawing/debugging junk
            _frontier = frontier;
            _visited = visited;

            Begin:
            lock (frontier)
                frontier.Enqueue(start);

            void Sleep()
            {
                Thread.Sleep(500);

                lock (frontier)
                    frontier.Clear();

                lock (visited)
                    visited.Clear();
            }

            while (frontier.Count > 0)
            {
                TilePosition current;
                lock (frontier)
                    current = frontier.Dequeue();

                if (world.TryGetChunk(ChunkPosition.FromTile(start), out Chunk startChunk))
                {
                    if (!startChunk.TryGetTile(end.LocalX, end.LocalY, out Tile tile) || tile.ID == 0)
                    {
                        Sleep();
                        goto Begin;
                    }
                }

                if (world.TryGetChunk(ChunkPosition.FromTile(end), out Chunk endChunk))
                {
                    if (!endChunk.TryGetTile(end.LocalX, end.LocalY, out Tile tile) || tile.ID == 0)
                    {
                        Sleep();
                        goto Begin;
                    }
                }

                for (int y = -1; y < 2; y++)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        var tilePos = new TilePosition(current.X + x, current.Y + y);
                        var chunkPos = ChunkPosition.FromTile(tilePos);
                        if (chunkPos.X < 0 || chunkPos.X >= 5 ||
                            chunkPos.Y < 0 || chunkPos.Y >= 5)
                            continue;

                        if (world.TryGetChunk(chunkPos, out Chunk chunk))
                        {
                            if (!visited.ContainsKey(tilePos) &&
                                chunk.GetTile(tilePos.LocalX, tilePos.LocalY).ID != 0)
                            {
                                lock (frontier)
                                    frontier.Enqueue(tilePos);

                                lock (visited)
                                    visited[tilePos] = true;

                                if (tilePos == end)
                                {
                                    if (Continue)
                                    {
                                        Sleep();
                                        goto Begin;
                                    }
                                }

                                Thread.Sleep(1);
                            }
                        }
                    }
                }
            }
        }
    }
}

