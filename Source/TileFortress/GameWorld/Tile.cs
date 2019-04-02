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

        public static bool operator ==(Tile tile1, Tile tile2)
        {
            return tile1.Equals(tile2);
        }

        public static bool operator !=(Tile tile1, Tile tile2)
        {
            return !tile1.Equals(tile2);
        }
    }
}
