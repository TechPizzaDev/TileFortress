using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace TileFortress.Net
{
    public static partial class BufferExtensions
    {
        public static Point ReadPoint32(this NetBuffer buffer)
        {
            int x = buffer.ReadInt32();
            int y = buffer.ReadInt32();
            return new Point(x, y);
        }
    }
}
