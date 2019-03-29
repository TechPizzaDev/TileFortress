using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace TileFortress.Net
{
    public static partial class BufferExtensions
    {
        public static void Write(this NetBuffer buffer, Point point)
        {
            buffer.Write(point.X);
            buffer.Write(point.Y);
        }
    }
}
