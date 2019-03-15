using Microsoft.Xna.Framework;
using System;

namespace Blueberry
{
    public static class VectorExt
    {
        public static void Set(this ref Vector2 v, float x, float y)
        {
            v.X = x;
            v.Y = y;
        }

        public static void Set(this ref Vector2 vt, Vector2 v)
        {
            vt.X = v.X;
            vt.Y = v.Y;
        }

        public static float Distance(this Vector2 vt, Vector2 v)
        {
            float x_d = v.X - vt.X;
            float y_d = v.X - vt.Y;
            return (float)Math.Sqrt(x_d * x_d + y_d * y_d);
        }
    }
}
