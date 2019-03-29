
namespace TileFortress.GameWorld
{
    public class Chunk
    {
        public const int Size = 16;

        private Tile[] _tiles;

        public Chunk()
        {
            _tiles = new Tile[Size * Size];
        }

        public bool GetIndex(int x, int y, out int index)
        {
            if (x < 0 || x > Size)
            {
                index = -1;
                return false;
            }

            if (y < 0 || y > Size)
            {
                index = -1;
                return false;
            }

            index = x + y * Size;
            return true;
        }

        public bool GetTile(int index, out Tile tile)
        {
            if (index < 0 || index >= _tiles.Length)
            {
                tile = default;
                return false;
            }

            tile = _tiles[index];
            return true;
        }

        public bool GetTile(int x, int y, out Tile tile)
        {
            if (GetIndex(x, y, out int index))
            {
                tile = _tiles[index];
                return true;
            }
            tile = default;
            return false;
        }

        public bool SetTile(int x, int y, Tile tile)
        {
            if (GetIndex(x, y, out int index))
            {
                _tiles[index] = tile;
                return true;
            }
            return false;
        }
    }
}
