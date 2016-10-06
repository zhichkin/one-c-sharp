using System;

namespace Zhichkin.Integrator.Translator
{
    public static class Utilities
    {
        public static int GetInt32(byte[] bytes)
        {
            byte[] value = new byte[4];
            bytes.CopyTo(value, 0);
            // If the system architecture is little-endian (that is, little end first), reverse the byte array.
            if (BitConverter.IsLittleEndian) Array.Reverse(value);
            return BitConverter.ToInt32(value, 0);
        }
        public static byte[] GetByteArray(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return bytes;
        }
    }
}
