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

        // https://medium.com/@nicholas.w.swift/easy-a-star-pathfinding-7e6689c7f7b2

        public readonly struct Node
        {
            public float G { get; }
            public float H { get; }
            public float F => G + H;

            public TilePosition Position { get; }

            public Node(float g, float h)
            {
                G = g;
                H = h;
            }

            public Node(TilePosition position)
            {
            }
        }

        public static Queue<Node> _openList;
        public static HashSet<Node> _closedList;
        public static bool Continue = true;

        public static void CreatePath(World world, TilePosition start)
        {
            var openList = new Queue<Node>();
            var closedList = new HashSet<Node>();

            Begin:
            openList.Enqueue(new Node(start));

            // debugging/drawing junk
            _openList = openList;
            _closedList = closedList;
            const int drawDist = 5;
            float velocity = 1;
            int tilesLeft = 1;

            while (openList.Count > 0)
            {
                //let the currentNode equal the node with the least f value
                //remove the currentNode from the openList
                //add the currentNode to the closedList

                var current = openList.Dequeue();
                for (int y = -1; y < 2; y++)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        var tilePos = new TilePosition(current.X + x, current.Y + y);
                        var chunkPos = ChunkPosition.FromTile(tilePos);
                        if (chunkPos.X < 0 || chunkPos.X >= drawDist ||
                            chunkPos.Y < 0 || chunkPos.Y >= drawDist)
                            continue;

                        if (world.TryGetChunk(chunkPos, out Chunk chunk))
                        {
                            bool added = false;

                            lock (closedList)
                            {
                                if (!closedList.ContainsKey(tilePos) &&
                                    chunk.GetTile(tilePos.LocalX, tilePos.LocalY).ID != 0)
                                {
                                    openList.Enqueue(tilePos);
                                    closedList[tilePos] = true;
                                    added = true;
                                }
                            }

                            if (added)
                            {
                                velocity += 0.00666f;

                                tilesLeft--;
                                if (tilesLeft <= 0)
                                {
                                    Thread.Sleep(8);
                                    tilesLeft = (int)velocity;
                                }
                            }
                        }
                    }
                }
            }

            Thread.Sleep(500);

            lock (_closedList)
                closedList.Clear();
            openList.Clear();


            if (Continue)
                goto Begin;
        }
    }
}

