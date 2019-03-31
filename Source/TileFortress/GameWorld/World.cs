using System;
using Microsoft.Xna.Framework;
using TileFortress.Utils;

namespace TileFortress.GameWorld
{
    public class World
    {
        public delegate void LoadDelegate(World sender);
        public event LoadDelegate OnLoad;
        public event LoadDelegate OnUnload;

        private SimplexNoise _noise;

        public World()
        {
            _noise = new SimplexNoise(123456);
        }

        public void Update(GameTime time)
        {
            
        }
        
        public bool TryGetChunk(ChunkPosition position, out Chunk chunk)
        {
            chunk = new Chunk(position);

            for (int y = 0; y < Chunk.Size; y++)
            {
                for (int x = 0; x < Chunk.Size; x++)
                {
                    int tx = x + chunk.Position.TileX;
                    int ty = y + chunk.Position.TileY;

                    int id = (int)Math.Floor(_noise.CalcPixel2D(tx, ty, 0.005f) / 256f * 3) + 1;
                    chunk.TrySetTile(x, y, new Tile((ushort)id));
                }
            }

            return true;
        }

        public void Load()
        {
            OnLoad?.Invoke(this);
        }

        public void Unload()
        {
            OnUnload?.Invoke(this);
        }
    }
}
