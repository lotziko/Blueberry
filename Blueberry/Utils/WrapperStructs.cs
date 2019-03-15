
using System;

namespace Blueberry
{
    public partial struct Mat
    {
    }

    public partial struct Col
    {
        private float r, g, b, a;
    }

    public partial struct Vec3
    {
        public float X, Y, Z;

        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public partial struct Rect
    {
        private float x, y, width, height;

        public float Left => x;
        public float Top => y;
        public float Right => x + width;
        public float Bottom => y + height;

        public void Set(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Contains(float x, float y)
        {
            x -= X;
            y -= Y;
            return (x >= 0 && x <= Width && y >= 0 && y <= Height);
        }

        public static bool operator ==(Rect value1, Rect value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y && value1.Width == value2.Width && value1.Height == value2.Height;
        }

        public static bool operator !=(Rect value1, Rect value2)
        {
            return value1.X != value2.X || value1.Y != value2.Y || value1.Width == value2.Width || value1.Height == value2.Height;
        }

        public override string ToString()
        {
            return string.Format("{{X:{0} Y:{1} Width:{2} Height:{3}}}", X, Y, Width, Height);
        }
    }

    public partial struct Vec2
    {
        public float X, Y;

        public static Vec2 Zero { get; } = new Vec2(0, 0);

        public Vec2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public void Set(Vec2 value)
        {
            X = value.X;
            Y = value.Y;
        }

        public void Set(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float Distance(Vec2 value)
        {
            float v1 = X - value.X, v2 = Y - value.Y;
            return (float)Math.Sqrt((v1 * v1) + (v2 * v2));
        }

        public static Vec2 operator -(Vec2 value1, Vec2 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            return value1;
        }

        public static Vec2 operator *(Vec2 value1, Vec2 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            return value1;
        }

        public static bool operator ==(Vec2 value1, Vec2 value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y;
        }

        public static bool operator !=(Vec2 value1, Vec2 value2)
        {
            return value1.X != value2.X || value1.Y != value2.Y;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
