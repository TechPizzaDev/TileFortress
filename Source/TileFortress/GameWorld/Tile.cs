
namespace TileFortress.GameWorld
{
    public readonly struct Tile
    {
        public ushort ID { get; }

        public Tile(ushort id)
        {
            ID = id;
        }
    }
}
