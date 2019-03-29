using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Threading;
using TileFortress.GameWorld;
using TileFortress.Server.Net;

namespace TileFortress.Server
{
    public class ServerGame
    {
        public delegate void LoadDelegate(ServerGame sender);
        public event LoadDelegate OnLoad;
        public event LoadDelegate OnUnload;

        private const int _updatesPerSecond = 40;
        private Ticker _ticker;
        private World _world;
        private NetGameServer _server;

        private bool ExitNextFrame { get; set; }

        public ServerGame(World world, NetGameServer server)
        {
            _ticker = new Ticker(this);
            _world = world;
            _server = server;
        }

        public void Update(GameTime time)
        {
            _world.Update(time);
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
            ExitNextFrame = true;
        }

        private class Ticker
        {
            private ServerGame _game;
            private GameTime _time;
            private Stopwatch _stopwatch;

            private TimeSpan _maxElapsed = TimeSpan.FromMilliseconds(250);
            private TimeSpan _targetRate;
            private TimeSpan _accumulatedTime;
            private long _previousTicks;

            public Ticker(ServerGame game)
            {
                _game = game;
                _time = new GameTime();
                _stopwatch = new Stopwatch();
            }

            public void Start(TimeSpan targetRate)
            {
                _targetRate = targetRate;

                _stopwatch.Start();
                while (Tick()) ;
                _stopwatch.Stop();
            }

            private bool Tick()
            {
                long currentTicks = _stopwatch.Elapsed.Ticks;
                _accumulatedTime += TimeSpan.FromTicks(currentTicks - _previousTicks);
                _previousTicks = currentTicks;

                if (_accumulatedTime < _targetRate)
                {
                    int sleepTime = (int)(_targetRate - _accumulatedTime).TotalMilliseconds;
                    Thread.Sleep(sleepTime);
                    return !_game.ExitNextFrame;
                }
                
                if (_accumulatedTime > _maxElapsed)
                    _accumulatedTime = _maxElapsed;

                _time.ElapsedGameTime = _accumulatedTime;
                _time.TotalGameTime += _accumulatedTime;
                _accumulatedTime = TimeSpan.Zero;

                _game.Update(_time);
                return !_game.ExitNextFrame;
            }
        }
    }
}
