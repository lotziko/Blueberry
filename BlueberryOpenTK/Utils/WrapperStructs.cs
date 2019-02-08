using OpenTK;
using OpenTK.Graphics;
using System;

namespace Blueberry
{
    public partial struct Mat
    {
        public Matrix4 m;

        public Mat(Matrix4 m)
        {
            this.m = m;
        }

        public static void Transform(ref float x, ref float y, Mat matrix)
        {
            float tX = x, tY = y;
            x = (tX * matrix.m.M11) + (tY * matrix.m.M21) + matrix.m.M41;
            y = (tX * matrix.m.M12) + (tY * matrix.m.M22) + matrix.m.M42;
        }

        public static Mat Identity { get; } = new Mat(Matrix4.Identity);
    }

    public partial struct Col
    {
        internal Color4 c;

        public float R
        {
            get => r;
            set
            {
                r = value;
                c.R = r;
            }
        }
        public float G
        {
            get => g;
            set
            {
                g = value;
                c.G = g;
            }
        }
        public float B
        {
            get => b;
            set
            {
                b = value;
                c.B = b;
            }
        }
        public float A
        {
            get => a;
            set
            {
                a = value;
                c.A = a;
            }
        }

        public Col(byte r, byte g, byte b, byte a)
        {
            this.r = r / 255f;
            this.g = g / 255f;
            this.b = b / 255f;
            this.a = a / 255f;
            c = new Color4(this.r, this.g, this.b, this.a);
        }

        public Col(Col col)
        {
            r = col.R;
            g = col.G;
            b = col.B;
            a = col.A;
            c = new Color4(r, g, b, a);
        }

        public Col(Col col, float alpha)
        {
            r = col.R;
            g = col.G;
            b = col.B;
            a = alpha;
            c = new Color4(r, g, b, a);
        }

        public Col(Color4 c)
        {
            r = c.R;
            g = c.G;
            b = c.B;
            a = c.A;
            this.c = new Color4(r, g, b, a);
        }

        public Col(Color4 c, float alpha)
        {
            r = c.R;
            g = c.G;
            b = c.B;
            a = alpha;
            this.c = new Color4(r, g, b, a);
        }

        public static bool operator ==(Col value1, Col value2)
        {
            return value1.c.R == value2.c.R && value1.c.G == value2.c.G && value1.c.B == value2.c.B && value1.c.A == value2.c.A;
        }

        public static bool operator !=(Col value1, Col value2)
        {
            return value1.c.R != value2.c.R || value1.c.G != value2.c.G || value1.c.B != value2.c.B || value1.c.A != value2.c.A;
        }

        public static Col Red { get; } = new Col(Color4.Red);
        public static Col White { get; } = new Col(Color4.White);
        public static Col Gray { get; } = new Col(Color4.Gray);
        public static Col DarkGray { get; } = new Col(Color4.DarkGray);
        public static Col Transparent { get; } = new Col(new Color4(0, 0, 0, 0));
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

        public Vec2 Multiply(Mat mat)
        {
            float x = this.X * mat.m.M11 + this.Y * mat.m.M21 + mat.m.M31;
            float y = this.X * mat.m.M12 + this.Y * mat.m.M22 + mat.m.M32;
            this.X = x;
            this.X = y;
            return this;
        }
    }
}
