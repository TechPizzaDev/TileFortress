using System;

namespace TileFortress.GameWorld
{
    public class Chunk
    {
        public const int Size = 16;

        private Tile[] _tiles;

        public ChunkPosition Position { get; }

        public Chunk(ChunkPosition position)
        {
            Position = position;

            _tiles = new Tile[Size * Size];
        }

        public void Fill(Tile tile)
        {
            for (int i = 0; i < _tiles.Length; i++)
                _tiles[i] = tile;
        }

        #region Get Tile
        public Tile GetTile(int index)
        {
            return _tiles[index];
        }

        public Tile GetTile(int x, int y)
        {
            int index = x + y * Size;
            return _tiles[index];
        }

        public bool TryGetTile(int index, out Tile tile)
        {
            if (index < 0 || index >= _tiles.Length)
            {
                tile = default;
                return false;
            }
            tile = _tiles[index];
            return true;
        }

        public bool TryGetTile(int x, int y, out Tile tile)
        {
            int index = x + y * Size;
            return TryGetTile(index, out tile);
        }

        public ReadOnlySpan<Tile> GetTileSpan()
        {
            return new ReadOnlySpan<Tile>(_tiles);
        }
        #endregion

        #region Set Tile
        public void SetTile(int index, Tile tile)
        {
            _tiles[index] = tile;
        }

        public void SetTile(int x, int y, Tile tile)
        {
            int index = x + y * Size;
            SetTile(index, tile);
        }

        public bool TrySetTile(int index, Tile tile)
        {
            if (index < 0 || index >= _tiles.Length)
                return false;
            _tiles[index] = tile;
            return true;
        }

        public bool TrySetTile(int x, int y, Tile tile)
        {
            int index = x + y * Size;
            return TrySetTile(index, tile);
        }
        #endregion

        public override string ToString()
        {
            return "Chunk " + Position;
        }
    }
}
