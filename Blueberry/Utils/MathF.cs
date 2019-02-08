using System;

namespace Blueberry
{
    public static class MathF
    {
        public static float Round(float value)
        {
            return (float)Math.Round(value);
        }

        public static float Sin(double a)
        {
            return (float)Math.Sin(a);
        }

        public static float Cos(double a)
        {
            return (float)Math.Cos(a);
        }

        public static float Abs(float value)
        {
            if (value < 0)
                return value * -1;
            return value;
        }

        public static float Floor(float value)
        {
            return (float)Math.Floor(value);
        }

        public static float Clamp(float value, float min, float max)
        {
            return value <= min ? min : (value >= max ? max : value);
        }

        public static int Clamp(int value, int min, int max)
        {
            return value <= min ? min : (value >= max ? max : value);
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">The angle in degrees.</param>
        /// <returns>The angle in radians.</returns>
        /// <remarks>
        /// This method uses double precision internally, though it returns single float.
        /// Factor = pi / 180
        /// </remarks>
        public static float ToRadians(float degrees)
        {
            return (float)(degrees * 0.017453292519943295769236907684886);
        }

        public static int NextPowerOfTwo(int value)
        {
            if (value == 0) return 1;
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return value + 1;
        }

        public static bool IsPowerOfTwo(int value)
        {
            return value != 0 && (value & value - 1) == 0;
        }
    }
}
