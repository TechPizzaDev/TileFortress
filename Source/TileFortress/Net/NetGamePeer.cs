using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

using Lidgren.Network;

namespace TileFortress.Net
{
    public abstract class NetGamePeer<TPeer> : IDisposable where TPeer : NetPeer
    {
        public delegate void StateDelegate(NetGamePeer<TPeer> sender);
        public delegate void ErrorDelegate(NetGamePeer<TPeer> sender, NetIncomingMessage message, object error);

        public event StateDelegate OnOpen;
        public event StateDelegate OnClose;
        public event ErrorDelegate OnError;

        private ConcurrentQueue<NetIncomingMessage> _messageQueue;

        public bool IsDisposed { get; private set; }

        protected TPeer Peer { get; }
        public int Port => Peer.Port;

        public NetGamePeer(TPeer peer)
        {
            Peer = peer;
            Peer.RegisterReceivedCallback(ReceiveMessage, new SynchronizationContext());
            _messageQueue = new ConcurrentQueue<NetIncomingMessage>();
        }

        #region Data Messages
        private void ReadDataMessage(NetIncomingMessage msg)
        {
            var type = msg.ReadEnum<DataMessageType>();
            switch (type)
            {
                case DataMessageType.ChatMessage:
                    string value = msg.ReadString();
                    OnChatMessage(msg, value);
                    break;
            }
            OnDataMessage(msg, type);
        }

        protected virtual void OnDataMessage(NetIncomingMessage message, DataMessageType type)
        {
        }

        protected virtual void OnChatMessage(NetIncomingMessage message, string value)
        {
            var endPoint = message.SenderConnection.RemoteEndPoint;
            Log.Info($"Chat [{endPoint}]: {value}");
        }
        #endregion

        #region Peer Messages
        protected virtual void OnMessage(NetIncomingMessage message)
        {
        }

        protected virtual void OnUnconnectedData(NetIncomingMessage message)
        {
        }

        protected virtual void OnStatusChange(NetIncomingMessage message, NetConnectionStatus status)
        {
            Log.Debug(status + ": " + message.ReadString());
        }
        #endregion

        #region Peer Methods
        protected NetOutgoingMessage CreateMessage(DataMessageType type)
        {
            var msg = Peer.CreateMessage();
            msg.Write(type);
            return msg;
        }

        private void ReceiveMessage(object state)
        {
            var peer = state as TPeer;
            var msg = peer.ReadMessage();

            if (msg != null)
            {
                lock (_messageQueue)
                    _messageQueue.Enqueue(msg);
            }
        }

        public int ReadMessages()
        {
            int count = 0;
            while (_messageQueue.TryDequeue(out NetIncomingMessage msg))
            {
                try
                {
                    count++;
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.Data:
                            ReadDataMessage(msg);
                            break;

                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = msg.ReadStatus();
                            OnStatusChange(msg, status);
                            break;

                        case NetIncomingMessageType.UnconnectedData:
                            OnUnconnectedData(msg);
                            break;

                        default:
                            OnMessage(msg);
                            break;
                    }
                }
                catch (Exception exc)
                {
                    OnError?.Invoke(this, msg, exc);
                }
                Peer.Recycle(msg);
            }
            return count;
        }

        protected void SendMessage(
            NetOutgoingMessage message, 
            NetConnection recipient, 
            NetDeliveryMethod method,
            DataSequenceChannel channel = DataSequenceChannel.Default)
        {
            Peer.SendMessage(message, recipient, method, (int)channel);
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
                if (sleeps > (Peer.Configuration.ConnectionTimeout + 1f) * 1000)
                    break;
            }

            OnClose?.Invoke(this);
        }
        #endregion

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    Close();
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
        #endregion
    }
}