using GeneralShare;
using System;
using System.Threading;
using TileFortress.GameWorld;
using TileFortress.Server.Net;

namespace TileFortress.Server
{
    class Program
    {
        private static Thread _thread;
        private static ServerGame _game;

        static void Main(string[] args)
        {
            Log.Initialize("log.txt");

            try
            {
                AppInfo.SetType(AppType.Server);
                AppInfo.SetCultureInfo();

                Run();

                Log.LineBreak();
                Log.Info("Successful exit after " + DebugUtils.TimeSinceStart.ToPreciseString(), false);
            }
            catch(Exception exc)
            {
                Log.Error(new Exception("Uncaught exception during execution.", exc));
            }
            finally
            {
                Log.Close();
            }
        }

        static void Run()
        {
            Log.Info("Starting server, version " + AppInfo.Version);
            Console.Title = "Tile Fortress Server " + AppInfo.Version;

            _thread = new Thread(MainThread);
            _thread.Priority = ThreadPriority.AboveNormal;
            _thread.IsBackground = false;
            _thread.Start();

            Log.Info("Use help for available commands");
            Log.LineBreak();

            while (true)
            {
                string line = Console.ReadLine().Trim();
                if (line.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Exit();
                    break;
                }
                else if (line.Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    Log.Info("\n exit: save and exit\n");
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    Log.Info($"Invalid command: \"{line}\", use \"help\" for available commands");
                }
            }
        }

        static void MainThread()
        {
            var server = new NetGameServer(AppConstants.NetDefaultPort);
            server.OnOpen += (s) => Log.Info("Server listening on port " + s.Port);
            server.OnClose += (s) => Log.Info("Listener closed");

            var world = new World();
            world.OnUnload += (w) => Log.Info("World unloaded");

            _game = new ServerGame(world, server);
            _game.Run();
        }

        static void Exit()
        {
            _game.Exit();
            if (!_thread.Join(20000))
            {
                throw new TimeoutException(
                    "Thread did not stop in requested time frame.");
            }
        }
    }
}
