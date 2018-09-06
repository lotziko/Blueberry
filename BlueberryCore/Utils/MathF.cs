using System;

namespace BlueberryCore
{
    public static class MathF
    {
        public static float Round(float value)
        {
            return (float) Math.Round(value);
        }

        public static float Sin(double a)
        {
            return (float)Math.Sin(a);
        }

        public static float Cos(double a)
        {
            return (float)Math.Cos(a);
        }
    }
}
