using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using TileFortress.GameWorld;
using TileFortress.Net;
using TileFortress.Server.Net;

namespace TileFortress.Server
{
    public partial class ServerGame
    {
        private const int UpdatesPerSecond = 40;

        public delegate void LoadDelegate(ServerGame sender);
        public event LoadDelegate OnLoad;
        public event LoadDelegate OnUnload;

        private Ticker _ticker;
        private World _world;
        private NetGameServer _server;

        private Queue<BuildOrder> _bufferedOrders = new Queue<BuildOrder>();

        public ServerGame(NetGameServer server)
        {
            _ticker = new Ticker(this);
            _server = server;

            _world = new World(World_ChunkRequest);
            _world.OnUnload += (w) => Log.Info("World unloaded");
            _world.OnBuildOrder += World_OnBuildOrder;
        }

        private void World_OnBuildOrder(World sender, Chunk chunk, BuildOrder order)
        {
            _bufferedOrders.Enqueue(order);
        }

        private Chunk World_ChunkRequest(World sender, ChunkPosition position)
        {
            var chunk = new Chunk(position);
            for (int y = 0; y < Chunk.Size; y++)
            {
                for (int x = 0; x < Chunk.Size; x++)
                {
                    int tx = x + chunk.Position.TileX;
                    int ty = y + chunk.Position.TileY;

                    int id = (int)Math.Floor(sender.Noise.CalcPixel2D(tx, ty, 0.005f) / 256f * 3) + 1;
                    chunk.TrySetTile(x, y, new Tile((ushort)id));
                }
            }
            return chunk;
        }

        private float _lol;
        private Random _rng = new Random();

        public void Update(GameTime time)
        {
            _server.ReadMessages();

            _world.Update(time);

            _lol += time.Delta;
            while(_lol > 0.01f)
            {
                _lol -= 0.01f;
                if (_lol > 0.5f)
                    _lol = 0.5f;

                int drawDist = 5;
                int cx = _rng.Next(drawDist);
                int cy = _rng.Next(drawDist);
                if (_world.TryGetChunk(new ChunkPosition(cx, cy), out Chunk chunk))
                {
                    int index = _rng.Next(Chunk.Size * Chunk.Size);
                    int id = _rng.Next(1, 4);
                    chunk.SetTile(index, new Tile((ushort)id));
                }
            }

            ProcessNetworkingResponses();
        }

        #region Post world-update networking
        private void ProcessNetworkingResponses()
        {
            SendChunks();
            _server.SendBuildOrders(_bufferedOrders);
        }

        private void SendChunks()
        {
            int maxRequests = 250;

            while (_server.ChunkRequests.Count > 0)
            {
                ChunkRequest request = _server.ChunkRequests.Dequeue();
                if (_world.TryGetChunk(request.Position, out Chunk chunk))
                {
                    _server.SendChunk(request.Sender, chunk);

                    if (maxRequests > 0)
                        maxRequests--;
                    else
                        break;
                }
            }
        }
        #endregion

        public void Run()
        {
            _world.Load();
            _server.Open();
            OnLoad?.Invoke(this);

            double ticksPerFrame = 1d / UpdatesPerSecond * TimeSpan.TicksPerSecond;
            _ticker.Start(TimeSpan.FromTicks((long)ticksPerFrame));
            
            _server.Close();
            _world.Unload();
            OnUnload?.Invoke(this);
        }

        public void Exit()
        {
            _ticker.Stop();
        }
    }
}
