using Microsoft.Xna.Framework;
﻿using System;
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

        public static void BreadthFirstSearch(World world, TilePosition start)
        {
            var frontier = new Queue<TilePosition>();
            frontier.Enqueue(start);

            var visited = new Dictionary<TilePosition, bool>();

            _frontier = frontier;
            _visited = visited;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();
                for (int y = -1; y < 2; y++)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        var tilePos = new TilePosition(current.X + x, current.Y + y);
                        var chunkPos = ChunkPosition.FromTile(tilePos);
                        if (chunkPos.X < 0 || chunkPos.X > 6 ||
                            chunkPos.Y < 0 || chunkPos.Y > 6)
                            continue;

                        if (world.TryGetChunk(chunkPos, out Chunk chunk))
                        {
                            lock (visited)
                            {
                                if (!visited.ContainsKey(tilePos) && chunk.GetTile(tilePos.LocalX, tilePos.LocalY).ID != 3)
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
}

