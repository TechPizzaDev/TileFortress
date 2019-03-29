using Lidgren.Network;
using System;
using System.Threading;
using TileFortress.GameWorld;

namespace TileFortress.Net
{
    public abstract class NetGamePeer<TPeer> : IDisposable where TPeer : NetPeer
    {
        // TODO: pls use dis for compressing "big" messages
        // https://github.com/MiloszKrajewski/K4os.Compression.LZ4

        public delegate void StateDelegate(NetGamePeer<TPeer> sender);
        public delegate void ErrorDelegate(NetGamePeer<TPeer> sender, NetIncomingMessage message, object error);

        public event StateDelegate OnOpen;
        public event StateDelegate OnClose;
        public event ErrorDelegate OnError;

        public bool IsDisposed { get; private set; }

        protected TPeer Peer { get; }
        public int Port => Peer.Port;

        public NetGamePeer(TPeer peer)
        {
            Peer = peer;
            peer.RegisterReceivedCallback(ReceiveMessage, new SynchronizationContext());
        }

        private void ReceiveMessage(object state)
        {
            var peer = state as NetPeer;
            var msg = peer.ReadMessage();
            try
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.ConnectionApproval:
                        if (ApproveClient(msg))
                            msg.SenderConnection.Approve();
                        break;

                    case NetIncomingMessageType.UnconnectedData:
                        OnUnconnectedData(msg);
                        break;

                    case NetIncomingMessageType.Data:
                        ReadDataMessage(msg);
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = msg.ReadStatus();
                        OnStatusChange(msg, status);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception exc)
            {
                OnError?.Invoke(this, msg, exc);
            }
            peer.Recycle(msg);
        }

        private void ReadDataMessage(NetIncomingMessage msg)
        {
            var type = msg.ReadEnum<DataMessageType>();
            switch (type)
            {
                case DataMessageType.ChatMessage:
                    string value = msg.ReadString();
                    OnChatMessage(msg, value);
                    break;

                case DataMessageType.ChunkRequest:
                    var position = msg.ReadPoint32();
                    var cr = new ChunkRequest(position);
                    OnChunkRequest(msg, cr);
                    break;

                case DataMessageType.ChunkData:
                    Span<ushort> tiles = stackalloc ushort[Chunk.Size * Chunk.Size];

                    var cd = new ChunkData(tiles);
                    OnChunkData(msg, cd);
                    break;

                default:
                    OnDataMessage(msg, type);
                    break;
            }
        }

        protected abstract bool ApproveClient(NetIncomingMessage message);

        protected virtual void OnStatusChange(NetIncomingMessage message, NetConnectionStatus status)
        {
            Log.Debug(status + ": " + message.ReadString());
        }

        protected virtual void OnChatMessage(NetIncomingMessage message, string value)
        {
            var endPoint = message.SenderConnection.RemoteEndPoint;
            Log.Info($"Chat [{endPoint}]: {value}");
        }

        protected virtual void OnChunkData(NetIncomingMessage message, ChunkData data)
        {
        }

        protected virtual void OnChunkRequest(NetIncomingMessage message, ChunkRequest request)
        {
        }

        protected virtual void OnUnconnectedData(NetIncomingMessage message)
        {
        }

        protected virtual void OnDataMessage(NetIncomingMessage message, DataMessageType type)
        {
            Log.Debug(type + ": " + message.ReadString());
        }

        protected NetOutgoingMessage CreateMessage(DataMessageType type)
        {
            var msg = Peer.CreateMessage();
            msg.Write(type);
            return msg;
        }

        public void Open()
        {
            Peer.Start();
            OnOpen?.Invoke(this);
        }

        public void Close()
        {
            Peer.Shutdown("exit");

            int sleeps = 0;
            while (Peer.Socket != null)
            {
                Thread.Sleep(1);
                sleeps++;
                if (sleeps > Peer.Configuration.ConnectionTimeout * 1000)
                    break;
            }

            OnClose?.Invoke(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {

                }

                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NetGamePeer()
        {
            Dispose(false);
        }
    }
}