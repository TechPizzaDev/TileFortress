using Microsoft.Xna.Framework;
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TileFortress.GameWorld;

namespace TileFortress.Pathfinding
{
    public static class PathFinder1
    {
        //f = g + h
        //G is the distance between the current node and the start node.
        //H is the heuristic — estimated distance from the current node to the end node.
        //F is the total cost of the node.

        // https://medium.com/@nicholas.w.swift/easy-a-star-pathfinding-7e6689c7f7b2

        public class Node : IEquatable<Node>
        {
            public float g;
            public float h;
            public float f;
            public Node Parent { get; }
            public TilePosition Position { get; }

            public Node(Node parent, TilePosition position)
            {
                Parent = parent;
                Position = position;
            }

            public bool Equals(Node other)
            {
                return Position == other.Position;
            }
        }

        public static List<Node> _openList;
        public static List<Node> _closedList;
        public static bool Continue = true;

        public static void CreatePath(World world, TilePosition start, TilePosition destination)
        {
            var openList = new List<Node>();
            var closedList = new List<Node>();

            Begin:

            // debugging/drawing junk
            _openList = openList;
            _closedList = closedList;
            const int drawDist = 5;
            
            var startNode = new Node(null, start);
            var endNode = new Node(null, destination);

            lock(openList)
            openList.Add(startNode);

            while (openList.Count > 0)
            {
                //let the currentNode equal the node with the least f value
                //remove the currentNode from the openList
                //add the currentNode to the closedList

                var current_node = openList[0];
                int current_index = 0;
                for (int i = 0; i < openList.Count; i++)
                {
                    var item = openList[i];
                    if (item.f < current_node.f)
                    {
                        current_node = item;
                        current_index = i;
                    }
                }

                lock (openList)
                    openList.RemoveAt(current_index);

                lock (closedList)
                    closedList.Add(current_node);

                // Found the goal
                if (current_node.Equals(endNode))
                {
                    var path = new List<TilePosition>();
                    var current = current_node;
                    while (current != null)
                    {
                        path.Add(current.Position);
                        current = current.Parent;
                    }

                    // return path[::- 1]; // Return reversed path

                    Thread.Sleep(500);
                    Console.WriteLine("Found goal");

                    lock (_closedList)
                        closedList.Clear();

                    lock (openList)
                        openList.Clear();

                    if (Continue)
                        goto Begin;
                }

                var adjacents = new TilePosition[]
                {
                    new TilePosition(0, -1),
                    new TilePosition(0, 1),
                    new TilePosition(-1, 0),
                    new TilePosition(1, 0),
                    new TilePosition(-1, -1),
                    new TilePosition(-1, 1),
                    new TilePosition(1, -1),
                    new TilePosition(1, 1)
                };

                var children = new List<Node>();
                foreach (var adjacent in adjacents)
                {
                    // Get node position
                    var node_position = new TilePosition(
                        current_node.Position.X + adjacent.X,
                        current_node.Position.Y + adjacent.Y);

                    var chunkPos = ChunkPosition.FromTile(node_position);

                    // Make sure within range
                    if (chunkPos.X < 0 || chunkPos.X >= drawDist ||
                        chunkPos.Y < 0 || chunkPos.Y >= drawDist)
                        continue;

                    // Ensure walkable terrain
                    if (world.TryGetChunk(chunkPos, out Chunk chunk) &&
                        chunk.GetTile(node_position.LocalX, node_position.LocalY).ID != 0)
                    {
                        // Add new node
                        var newNode = new Node(current_node, node_position);
                        children.Add(newNode);
                    }
                }

                foreach (var child in children)
                {
                    // Child is on the closed list
                    lock (closedList)
                        if (closedList.Contains(child))
                            continue;

                    // Set the f, g, and h values
                    child.g = current_node.g + 1;
                    float xNum = child.Position.X - endNode.Position.X;
                    float yNum = child.Position.Y - endNode.Position.Y;
                    child.h = xNum * xNum + yNum * yNum;
                    child.f = child.g + child.h;

                    // Child is already in the open list
                    foreach (var open_node in openList)
                        if (child.Equals(open_node) && child.g > open_node.g)
                            continue;

                    // Add the child to the open list
                    lock (openList)
                        openList.Add(child);

                    Thread.Sleep(2);
                }
            }
        }
    }
}

