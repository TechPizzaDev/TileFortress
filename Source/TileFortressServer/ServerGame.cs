using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using TileFortress.GameWorld;
using TileFortress.Net;
using TileFortress.Server.Net;

namespace TileFortress.Server
{
    public partial class ServerGame
    {
        public delegate void LoadDelegate(ServerGame sender);
        public event LoadDelegate OnLoad;
        public event LoadDelegate OnUnload;

        private const int _updatesPerSecond = 20;
        private Ticker _ticker;
        private World _world;
        private NetGameServer _server;

        public ServerGame(World world, NetGameServer server)
        {
            _ticker = new Ticker(this);
            _world = world;
            _server = server;
        }

        public void Update(GameTime time)
        {
            _world.Update(time);

            ProcessNetworking();
        }

        private void ProcessNetworking()
        {
            ProcessChunkRequests();
        }

        private void ProcessChunkRequests()
        {
            var queue = _server.ChunkRequests;
            int maxRequests = 250;

            lock (queue)
            {
                while (queue.Count > 0 && maxRequests > 0)
                {
                    ChunkRequest request = queue.Dequeue();
                    if (_world.TryGetChunk(request.Position, out Chunk chunk))
                    {
                        _server.SendChunk(request.Sender, chunk);
                        maxRequests--;
                    }
                }
            }
        }

        public void Run()
        {
            _world.Load();
            _server.Open();
            OnLoad?.Invoke(this);

            double ticksPerFrame = 1d / _updatesPerSecond * TimeSpan.TicksPerSecond;
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
