
namespace TileFortress.GameWorld
{
    public readonly struct BuildOrder
    {
        public TilePosition Position { get; }
        public Tile Tile { get; }

        public BuildOrder(TilePosition position, Tile tile)
        {
            Tile = tile;
            Position = position;
        }
    }
}