using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;

namespace TileFortress.Server
{
    public partial class ServerGame
    {
        private class Ticker
        {
            public delegate void UpdateDelegate(GameTime time);

            private UpdateDelegate _delegate;
            private GameTime _time;
            private Stopwatch _stopwatch;
            private bool _exitNextFrame;

            private TimeSpan _maxElapsed = TimeSpan.FromMilliseconds(250);
            private TimeSpan _targetRate;
            private TimeSpan _accumulatedTime;
            private long _previousTicks;

            public Ticker(UpdateDelegate update)
            {
                _delegate = update;
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
                    return !_exitNextFrame;
                }

                if (_accumulatedTime > _maxElapsed)
                    _accumulatedTime = _maxElapsed;

                _time.ElapsedGameTime = _accumulatedTime;
                _time.TotalGameTime += _accumulatedTime;
                _accumulatedTime = TimeSpan.Zero;

                _delegate?.Invoke(_time);
                return !_exitNextFrame;
            }

            public void Stop()
            {
                _exitNextFrame = true;
            }
        }
    }
}
