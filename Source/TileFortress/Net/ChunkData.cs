using System;
using TileFortress.GameWorld;

namespace TileFortress.Net
{
    public readonly ref struct ChunkData
    {
        public ChunkPosition Position { get; }
        public ReadOnlySpan<Tile> Tiles { get; }

        public ChunkData(ChunkPosition position, Span<Tile> tiles)
        {
            Position = position;
            Tiles = tiles;
        }
    }
}
