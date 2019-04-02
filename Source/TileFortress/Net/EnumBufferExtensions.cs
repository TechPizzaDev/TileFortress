using GeneralShare;
using Lidgren.Network;
using System;
using System.Runtime.InteropServices;

namespace TileFortress.Net
{
    public static class EnumBufferExtensions
    {
        public static void Write<TEnum>(this NetOutgoingMessage output, TEnum value)
            where TEnum : struct, Enum, IConvertible
        {
            var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
            int size = Marshal.SizeOf(underlyingType);
            long valueBits = EnumConverter<TEnum>.Convert(value);

            switch (size)
            {
                case 1:
                    output.Write(valueBits, 8);
                    break;

                case 2:
                    output.Write(valueBits, 16);
                    break;

                case 4:
                    output.Write(valueBits, 32);
                    break;

                case 8:
                    output.Write(valueBits, 64);
                    break;

                default:
                    throw new InvalidCastException(
                        $"Could not find matching type for {size} bytes.");
            }
        }
        
        public static TEnum ReadEnum<TEnum>(this NetIncomingMessage input)
            where TEnum : struct, Enum
        {
            var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
            int size = Marshal.SizeOf(underlyingType);
            switch (size)
            {
                case 1:
                    byte i8 = input.ReadByte();
                    return EnumConverter<TEnum>.Convert(i8);

                case 2:
                    short i16 = input.ReadInt16();
                    return EnumConverter<TEnum>.Convert(i16);

                case 4:
                    int i32 = input.ReadInt32();
                    return EnumConverter<TEnum>.Convert(i32);

                case 8:
                    long i64 = input.ReadInt64();
                    return EnumConverter<TEnum>.Convert(i64);

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
