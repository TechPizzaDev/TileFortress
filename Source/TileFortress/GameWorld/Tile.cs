using System;

namespace TileFortress.GameWorld
{
    public readonly struct Tile : IEquatable<Tile>
    {
        public ushort ID { get; }

        public Tile(ushort id)
        {
            ID = id;
        }

        public bool Equals(Tile other)
        {
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            if (obj is Tile tile)
                return Equals(tile);
            return false;
        }

        public override int GetHashCode()
        {
            return ID;
        }

        #region Equality Operators
        public static bool operator ==(Tile a, Tile b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Tile a, Tile b)
        {
            return !a.Equals(b);
        }
        #endregion
    }
}
