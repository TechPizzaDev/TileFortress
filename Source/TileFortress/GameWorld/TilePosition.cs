using System;
using Microsoft.Xna.Framework;

namespace TileFortress.GameWorld
{
    public struct TilePosition : IEquatable<TilePosition>
    {
        public int X;
        public int Y;

        public int ChunkX => (int)Math.Floor((double)X / Chunk.Size);
        public int ChunkY => (int)Math.Floor((double)Y / Chunk.Size);

        public int LocalX => Math.Abs(X % Chunk.Size);
        public int LocalY => Math.Abs(Y % Chunk.Size);

        public TilePosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static TilePosition FromIndex(int index)
        {
            return new TilePosition(
                index % Chunk.Size,
                index / Chunk.Size);
        }

        public bool Equals(TilePosition other)
        {
            return X == other.X
                && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (obj is TilePosition pos)
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

        #region Equality Operators
        public static bool operator ==(TilePosition a, TilePosition b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TilePosition a, TilePosition b)
        {
            return !a.Equals(b);
        }
        #endregion

        #region Implicit Operators
        public static implicit operator TilePosition(Point point)
        {
            return new TilePosition(point.X, point.Y);
        }

        public static implicit operator Point(TilePosition position)
        {
            return new Point(position.X, position.Y);
        }
        #endregion
    }
}