using Microsoft.Xna.Framework;

namespace TileFortress.GameWorld
{
    public struct ChunkPosition
    {
        public int X;
        public int Y;

        public int TileX => X * Chunk.Size;
        public int TileY => Y * Chunk.Size;

        public ChunkPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator ChunkPosition(Point point)
        {
            return new ChunkPosition(point.X, point.Y);
        }

        public static implicit operator Point(ChunkPosition position)
        {
            return new Point(position.X, position.Y);
        }

        public override string ToString()
        {
            return "X:" + X + " Y:" + Y;
        }
    }
}
