﻿
using Microsoft.Xna.Framework;

namespace Blueberry
{
    public partial struct Mat
    {
        public Matrix m;

        public Mat(Matrix m)
        {
            this.m = m;
        }

        public static void Transform(ref float x, ref float y, Mat matrix)
        {
            float tX = x, tY = y;
            x = (tX * matrix.m.M11) + (tY * matrix.m.M21) + matrix.m.M41;
            y = (tX * matrix.m.M12) + (tY * matrix.m.M22) + matrix.m.M42;
        }

        public static Mat Identity { get; } = new Mat(Matrix.Identity);
    }

    public partial struct Col
    {
        private Color c;

        public float R
        {
            get => r;
            set
            {
                r = value;
                c.R = System.Convert.ToByte(r * 255);
            }
        }
        public float G
        {
            get => g;
            set
            {
                g = value;
                c.R = System.Convert.ToByte(g * 255);
            }
        }
        public float B
        {
            get => b;
            set
            {
                b = value;
                c.R = System.Convert.ToByte(b * 255);
            }
        }
        public float A
        {
            get => a;
            set
            {
                a = value;
                c.R = System.Convert.ToByte(a * 255);
            }
        }

        public Col(Color c)
        {
            r = c.R / 255f;
            g = c.G / 255f;
            b = c.B / 255f;
            a = c.A / 255f;
            this.c = new Color(r, g, b, a);
        }

        public Col(Col col)
        {
            r = col.R;
            g = col.G;
            b = col.B;
            a = col.A;
            c = new Color(r, g, b, a);
        }

        public Col(Col col, float alpha)
        {
            r = col.R;
            g = col.G;
            b = col.B;
            a = alpha;
            c = new Color(r, g, b, a);
        }

        public Col(byte r, byte g, byte b, byte a)
        {
            this.r = r / 255f;
            this.g = g / 255f;
            this.b = b / 255f;
            this.a = a / 255f;
            this.c = new Color(r, g, b, a);
        }

        public static bool operator ==(Col value1, Col value2)
        {
            return value1.c.R == value2.c.R && value1.c.G == value2.c.G && value1.c.B == value2.c.B && value1.c.A == value2.c.A;
        }

        public static bool operator !=(Col value1, Col value2)
        {
            return value1.c.R != value2.c.R || value1.c.G != value2.c.G || value1.c.B != value2.c.B || value1.c.A != value2.c.A;
        }

        public static Col Transparent { get; } = new Col(Color.Transparent);
        public static Col White { get; } = new Col(Color.White);
        public static Col Gray { get; } = new Col(Color.Gray);
    }

    public partial struct Vec3
    {
       
    }

    public partial struct Vec2
    {
        public static Vec2 Transform(Vec2 position, Mat matrix)
        {
            Transform(ref position, ref matrix, out position);
            return position;
        }

        public static void Transform(ref Vec2 position, ref Mat matrix, out Vec2 result)
        {
            result = new Vec2((position.X * matrix.m.M11) + (position.Y * matrix.m.M21) + matrix.m.M41, (position.X * matrix.m.M12) + (position.Y * matrix.m.M22) + matrix.m.M42);
        }
    }
}