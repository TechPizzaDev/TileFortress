using Lidgren.Network;
using TileFortress.Net;
using System;
using System.Threading;
using System.Net;

namespace TileFortress.Client.Net
{
    public class NetGameClient : NetGamePeer<NetClient>
    {
        private AutoResetEvent _connectWaitHandle;

        public bool IsConnected => Peer.ConnectionStatus == NetConnectionStatus.Connected;

        public float Latency
        {
            get
            {
                if (Peer.ServerConnection != null)
                    return Peer.ServerConnection.AverageRoundtripTime;
                return -1;
            }
        }

        public NetGameClient() : base(CreatePeer())
        {
            _connectWaitHandle = new AutoResetEvent(false);
        }

        public bool Connect(IPEndPoint endPoint)
        {
            if (Peer.Status == NetPeerStatus.NotRunning)
                Open();

            NetOutgoingMessage hail = Peer.CreateMessage();
            hail.Write(AppInfo.Version.ToString());
            Peer.Connect(endPoint, hail);

            int timeout = (int)((Peer.Configuration.ConnectionTimeout + 1) * 1000);
            return _connectWaitHandle.WaitOne(timeout) 
                && Peer.ConnectionStatus == NetConnectionStatus.Connected;
        }

        public bool Connect(IPAddress address, int port)
        {
            return Connect(new IPEndPoint(address, port));
        }

        public void RequestChunk(ChunkRequest request)
        {
            var msg = CreateMessage(DataMessageType.ChunkRequest);
            msg.Write(request.Position);

            SendMessage(msg, NetDeliveryMethod.ReliableUnordered, DataSequenceChannel.Tiles);
        }

        protected void SendMessage(
            NetOutgoingMessage message, NetDeliveryMethod method, DataSequenceChannel channel)
        {
            Peer.SendMessage(message, method, (int)channel);
        }

        protected override void OnChunkData(NetIncomingMessage message, ChunkData data)
        {

        }

        protected override bool ApproveClient(NetIncomingMessage message)
        {
            return false;
        }

        protected override void OnStatusChange(NetIncomingMessage message, NetConnectionStatus status)
        {
            switch (status)
            {
                case NetConnectionStatus.Connected:
                    _connectWaitHandle.Set();
                    Log.Info("Connected to \"" + message.SenderConnection.RemoteEndPoint + "\"");
                    break;

                case NetConnectionStatus.Disconnected:
                    _connectWaitHandle.Set();
                    Log.Info("Disconnected from \"" + message.SenderConnection.RemoteEndPoint + "\"");
                    break;

                default:
                    base.OnStatusChange(message, status);
                    break;
            }
        }

        private static NetClient CreatePeer()
        {
            var config = new NetPeerConfiguration(AppConstants.NetAppIdentifier)
            {
                UseMessageRecycling = true,
                PingInterval = 2f
            };

            return new NetClient(config);
        }
    }
}
