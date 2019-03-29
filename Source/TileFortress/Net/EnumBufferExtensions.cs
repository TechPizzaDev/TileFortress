using GeneralShare;
using Lidgren.Network;
using System;
using System.Runtime.InteropServices;

namespace TileFortress.Net
{
    public static class EnumBufferExtensions
    {
        public static void Write<TEnum>(this NetOutgoingMessage output, TEnum value)
            where TEnum : Enum, IConvertible
        {
            int size = Marshal.SizeOf(Enum.GetUnderlyingType(typeof(TEnum)));
            switch (size)
            {
                case 1:
                    output.Write(0, 2);
                    output.Write(value.ToByte(null));
                    break;

                case 2:
                    output.Write(1, 2);
                    output.Write(value.ToInt16(null));
                    break;

                case 4:
                    output.Write(2, 2);
                    output.Write(value.ToInt32(null));
                    break;

                case 8:
                    output.Write(3, 2);
                    output.Write(value.ToInt64(null));
                    break;

                default:
                    throw new InvalidCastException(
                        $"Could not find matching type for {size} bytes.");
            }
        }
        
        public static TEnum ReadEnum<TEnum>(this NetIncomingMessage input)
            where TEnum : Enum, IConvertible
        {
            int type = input.ReadInt32(2);
            switch (type)
            {
                case 0:
                    byte i8 = input.ReadByte();
                    return EnumConverter<TEnum>.Convert(i8);

                case 1:
                    short i16 = input.ReadInt16();
                    return EnumConverter<TEnum>.Convert(i16);

                case 2:
                    int i32 = input.ReadInt32();
                    return EnumConverter<TEnum>.Convert(i32);

                case 3:
                    long i64 = input.ReadInt64();
                    return EnumConverter<TEnum>.Convert(i64);
            }
            throw new InvalidOperationException();
        }
    }
}
