using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using K4os.Compression.LZ4;
using Lidgren.Network;
using TileFortress.GameWorld;
using TileFortress.Net;

namespace TileFortress.Server.Net
{
    public class NetGameServer : NetGamePeer<NetServer>
    {
        public Queue<ChunkRequest> ChunkRequests = new Queue<ChunkRequest>();

        public NetGameServer(int port) : base(CreatePeer(port))
        {

        }

        #region Client Methods
        private void OnClientApproval(NetIncomingMessage message)
        {
            var connection = message.SenderConnection;
            connection.Approve();
        }

        private void OnClientConnect(NetConnection connection)
        {

        }

        private void OnClientDisconnect(NetConnection connection)
        {

        }
        #endregion

        #region Data Message Handlers
        private void OnChunkRequest(ChunkRequest request)
        {
            lock (ChunkRequests)
                ChunkRequests.Enqueue(request);
        }
        #endregion

        #region Data Send Methods
        public void SendChunk(NetConnection recipient, Chunk chunk)
        {
            var watch = Stopwatch.StartNew();

            ReadOnlySpan<Tile> tiles = chunk.GetTileSpan();
            ReadOnlySpan<byte> tileBytes = MemoryMarshal.AsBytes(tiles);

            int maxCompressedTileBytes = LZ4Codec.MaximumOutputSize(tileBytes.Length);
            Span<byte> compressedChunkBytes = stackalloc byte[maxCompressedTileBytes];
            int length = LZ4Codec.Encode(tileBytes, compressedChunkBytes);

            var msg = CreateMessage(DataMessageType.ChunkData);
            msg.Write(chunk.Position);
            msg.Write((ushort)length);
            msg.Write(compressedChunkBytes.Slice(0, length));

            watch.Stop();

            SendMessage(msg, recipient, NetDeliveryMethod.ReliableUnordered, DataSequenceChannel.Tiles);
            //Log.Debug("Sent " + chunk + " to " + recipient.RemoteEndPoint + " in " + watch.Elapsed.TotalMilliseconds.ToString("0.00") + "ms");
        }
        #endregion

        #region Peer Methods
        protected override void OnDataMessage(NetIncomingMessage message, DataMessageType type)
        {
            switch (type)
            {
                case DataMessageType.ChunkRequest:
                    {
                        var position = message.ReadPoint32();
                        var request = new ChunkRequest(position, message.SenderConnection);
                        if (request.Sender == null)
                            throw new Exception();
                        OnChunkRequest(request);
                        break;
                    }
            }
        }

        protected override void OnStatusChange(NetIncomingMessage message, NetConnectionStatus status)
        {
            switch (status)
            {
                case NetConnectionStatus.RespondedConnect:
                case NetConnectionStatus.RespondedAwaitingApproval:
                    break;

                case NetConnectionStatus.Connected:
                    OnClientConnect(message.SenderConnection);
                    Log.Info($"\"{message.SenderConnection.RemoteEndPoint}\" connected");
                    break;

                case NetConnectionStatus.Disconnected:
                    OnClientDisconnect(message.SenderConnection);
                    Log.Info($"\"{message.SenderConnection.RemoteEndPoint}\" disconnected");
                    break;

                default:
                    base.OnStatusChange(message, status);
                    break;
            }
        }

        protected override void OnMessage(NetIncomingMessage message)
        {
            switch (message.MessageType)
            {
                case NetIncomingMessageType.ConnectionApproval:
                    OnClientApproval(message);
                    break;
            }
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
        #endregion
    }
}
