using System;
using Microsoft.Xna.Framework;

namespace TileFortress.GameWorld
{
    public struct ChunkPosition : IEquatable<ChunkPosition>
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

        public static ChunkPosition FromTile(int x, int y)
        {
            return new ChunkPosition(
                (int)Math.Floor((double)x / Chunk.Size),
                (int)Math.Floor((double)y / Chunk.Size));
        }

        public static ChunkPosition FromTile(TilePosition position)
        {
            return FromTile(position.X, position.Y);
        }

        public bool Equals(ChunkPosition other)
        {
            return X == other.X
                && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (obj is ChunkPosition pos)
                return Equals(pos);
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + X;
                hash = hash * 31 + Y;
                return hash;
            }
        }

        public override string ToString()
        {
            return "X:" + X + " Y:" + Y;
        }

        #region Implicit Operators
        public static implicit operator ChunkPosition(Point point)
        {
            return new ChunkPosition(point.X, point.Y);
        }

        public static implicit operator Point(ChunkPosition position)
        {
            return new Point(position.X, position.Y);
        }

        public static implicit operator ChunkPosition(TilePosition position)
        {
            return new ChunkPosition(position.ChunkX, position.ChunkY);
        }
        #endregion
    }
}
