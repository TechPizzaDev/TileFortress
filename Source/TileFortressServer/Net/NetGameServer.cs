using Lidgren.Network;
using TileFortress.Net;

namespace TileFortress.Server.Net
{
    public class NetGameServer : NetGamePeer<NetServer>
    {
        public NetGameServer(int port) : base(CreatePeer(port))
        {
        }

        protected override bool ApproveClient(NetIncomingMessage message)
        {
            return true;
        }

        protected override void OnStatusChange(NetIncomingMessage message, NetConnectionStatus status)
        {
            switch (status)
            {
                case NetConnectionStatus.RespondedConnect:
                case NetConnectionStatus.RespondedAwaitingApproval:
                    break;

                case NetConnectionStatus.Connected:
                    Log.Info($"\"{message.SenderConnection.RemoteEndPoint}\" connected");
                    break;

                case NetConnectionStatus.Disconnected:
                    Log.Info($"\"{message.SenderConnection.RemoteEndPoint}\" disconnected");
                    break;

                default:
                    base.OnStatusChange(message, status);
                    break;
            }
        }

        protected override void OnChunkRequest(NetIncomingMessage message, ChunkRequest request)
        {
            Log.Debug("A CHUNKY REQUESTE: " + request.Position);
        }

        private static NetServer CreatePeer(int port)
        {
            var config = new NetPeerConfiguration(AppConstants.NetAppIdentifier)
            {
                Port = port,
                AcceptIncomingConnections = true,
                UseMessageRecycling = true,
                PingInterval = 2f
            };
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);

            return new NetServer(config);
        }
    }
}
