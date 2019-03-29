using Microsoft.Xna.Framework;

namespace TileFortress.Net
{
    public readonly struct ChunkRequest
    {
        public Point Position { get; }

        public ChunkRequest(Point position)
        {
            Position = position;
        }
    }
}
