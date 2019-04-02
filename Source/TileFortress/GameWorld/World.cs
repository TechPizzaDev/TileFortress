using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TileFortress.Utils;

namespace TileFortress.GameWorld
{
    public class World
    {
        public delegate void LoadDelegate(World sender);
        public event LoadDelegate OnLoad;
        public event LoadDelegate OnUnload;

        public delegate void BuildOrderDelegate(World sender, Chunk chunk, BuildOrder order);
        public event BuildOrderDelegate OnBuildOrder;

        public delegate Chunk ChunkRequestDelegate(World sender, ChunkPosition position);
        public readonly ChunkRequestDelegate ChunkRequest;

        public SimplexNoise Noise;
        private Dictionary<ChunkPosition, Chunk> _chunks;
        private Queue<BuildOrder> _buildOrders;

        public World(ChunkRequestDelegate chunkRequest)
        {
            ChunkRequest = chunkRequest ?? throw new ArgumentNullException(nameof(chunkRequest));

            Noise = new SimplexNoise(123456);
            _chunks = new Dictionary<ChunkPosition, Chunk>();
            _buildOrders = new Queue<BuildOrder>();
        }

        public void Update(GameTime time)
        {
            ApplyBuildOrders();
        }

        private void ApplyBuildOrders()
        {
            var tmp = new Stack<BuildOrder>();

            while (_buildOrders.Count > 0)
            {
                BuildOrder order = _buildOrders.Dequeue();
                ChunkPosition chunkPos = order.Position;
                if (TryGetChunk(chunkPos, out Chunk chunk))
                {
                    chunk.TrySetTile(order.Position, order.Tile);
                    OnBuildOrder?.Invoke(this, chunk, order);
                }
                else
                    tmp.Push(order);
            }

            while(tmp.Count > 0)
            {
                BuildOrder order = tmp.Pop();
                _buildOrders.Enqueue(order);
            }
        }

        public void EnqueueBuildOrder(BuildOrder order)
        {
            _buildOrders.Enqueue(order);
        }

        public bool TryGetChunk(ChunkPosition position, out Chunk chunk)
        {
            if (!_chunks.TryGetValue(position, out chunk))
            {
                chunk = ChunkRequest.Invoke(this, position);
                if (chunk == null)
                    return false;

                _chunks.Add(position, chunk);
                chunk.OnChunkUpdate += Chunk_OnChunkUpdate;
            }
            return true;
        }

        private void Chunk_OnChunkUpdate(Chunk sender, int index)
        {
            var position = TilePosition.FromIndex(index);
            position.X += sender.Position.TileX;
            position.Y += sender.Position.TileY;

            var tile = sender.GetTile(index);
            var order = new BuildOrder(position, tile);

            OnBuildOrder?.Invoke(this, sender, order);
        }

        public void Load()
        {
            OnLoad?.Invoke(this);
        }

        public void Unload()
        {
            OnUnload?.Invoke(this);
        }
    }
}
