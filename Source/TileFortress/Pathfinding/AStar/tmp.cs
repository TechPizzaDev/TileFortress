using System;
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

        public static List<Node> _frontier;
        public static HashSet<TilePosition> _visited;
        public static bool Continue = true;

        public class DistanceToEndComparer : IComparer<Node>
        {
            public TilePosition End { get; }

            public DistanceToEndComparer(TilePosition end)
            {
                End = end;
            }

            public int Compare(Node x, Node y)
            {
                float xF = x.Position.DistanceSquared(End) + x.Cost;
                float yF = y.Position.DistanceSquared(End) + y.Cost;
                return yF.CompareTo(xF);
            }
        }

        public readonly struct Node
        {
            public TilePosition Position { get; }
            public int Cost { get; }
            
            public Node(TilePosition position, int cost)
            {
                Position = position;
                Cost = cost;
            }
        }

        public static void BreadthFirstSearch(World world, TilePosition start, TilePosition end)
        {
            var comparer = new DistanceToEndComparer(end);
            var frontier = new List<Node>();
            var visited = new HashSet<TilePosition>();

            // drawing/debugging junk
            _frontier = frontier;
            _visited = visited;

            Begin:

            lock (frontier)
            {
                frontier.Add(new Node(start, 0));
                frontier.Sort(comparer);
            }

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
                Node current = frontier[frontier.Count - 1];
                lock (frontier)
                    frontier.RemoveAt(frontier.Count - 1);

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

                var adjacents = new TilePosition[8]
                {
                    new TilePosition(-1, 1),
                    new TilePosition(0, 1),
                    new TilePosition(1, 1),

                    new TilePosition(-1, 0),
                    new TilePosition(1, 0),

                    new TilePosition(-1, -1),
                    new TilePosition(0, -1),
                    new TilePosition(1, -1),
                };

                foreach (var a in adjacents)
                {
                    var tilePos = new TilePosition(current.Position.X + a.X, current.Position.Y + a.Y);
                    var chunkPos = ChunkPosition.FromTile(tilePos);
                    if (chunkPos.X < 0 || chunkPos.X >= 12 ||
                        chunkPos.Y < 0 || chunkPos.Y >= 12)
                        continue;

                    if (world.TryGetChunk(chunkPos, out Chunk chunk))
                    {
                        if (!visited.Contains(tilePos) &&
                            chunk.GetTile(tilePos.LocalX, tilePos.LocalY).ID != 0)
                        {
                            lock (frontier)
                            {
                                frontier.Add(new Node(tilePos, current.Cost + 1));
                                frontier.Sort(comparer);
                            }

                            lock (visited)
                                visited.Add(tilePos);

                            if (tilePos == end)
                            {
                                if (Continue)
                                {
                                    Sleep();
                                    goto Begin;
                                }
                            }

                            if (a.X == 0)
                                Thread.Sleep(1);
                        }
                    }
                }
            }

            Sleep();
            goto Begin;
        }
    }
}

