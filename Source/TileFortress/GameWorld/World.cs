using Microsoft.Xna.Framework;

namespace TileFortress.GameWorld
{
    public class World
    {
        public delegate void LoadDelegate(World sender);
        public event LoadDelegate OnLoad;
        public event LoadDelegate OnUnload;

        public void Update(GameTime time)
        {
            
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
