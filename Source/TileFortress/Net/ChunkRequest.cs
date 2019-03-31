using Lidgren.Network;
using TileFortress.GameWorld;

namespace TileFortress.Net
{
    public readonly struct ChunkRequest
    {
        public ChunkPosition Position { get; }
        public NetConnection Sender { get; }

        public ChunkRequest(ChunkPosition position, NetConnection sender)
        {
            Position = position;
            Sender = sender;
        }

        public ChunkRequest(ChunkPosition position) : this(position, sender: null)
        {
        }
    }
}
