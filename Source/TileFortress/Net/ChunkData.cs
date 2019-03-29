using System;

namespace TileFortress.Net
{
    public readonly ref struct ChunkData
    {
        public Span<ushort> Tiles { get; }

        public ChunkData(Span<ushort> tiles)
        {
            Tiles = tiles;
        }
    }
}
