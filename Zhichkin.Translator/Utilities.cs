using System;

namespace Zhichkin.Integrator.Translator
{
    public static class Utilities
    {
        public static int GetInt32(byte[] bytes)
        {
            // If the system architecture is little-endian (that is, little end first), reverse the byte array.
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
