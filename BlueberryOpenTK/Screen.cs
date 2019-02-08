using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry
{
    public static partial class Screen
    {
        private static int width, height;
        private static Mat projection;

        public static int Width
        {
            get => width;
            set
            {
                width = value;
                projection.m = OpenTK.Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);
            }
        }
        public static int Height
        {
            get => height;
            set
            {
                height = value;
                projection.m = OpenTK.Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);
            }
        }
        public static Rect Bounds { get; }

        public static Mat DefaultProjection => projection;
        public static Mat DefaultTransform { get; } = new Mat(OpenTK.Matrix4.Identity);
    }
}
