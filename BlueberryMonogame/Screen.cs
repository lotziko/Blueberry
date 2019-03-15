using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry
{
    public static partial class Screen
    {
        private static int width, height;
        private static Mat projection;
        private static Rect bounds;

        public static int Width
        {
            get => width;
            set
            {
                width = value;
                bounds.Width = value;
                projection.m = Matrix.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);
            }
        }
        public static int Height
        {
            get => height;
            set
            {
                height = value;
                bounds.Height = value;
                projection.m = Matrix.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);
            }
        }
        public static Rect Bounds => bounds;

        public static Mat DefaultProjection => projection;
        public static Mat DefaultTransform { get; } = new Mat(Matrix.Identity);
    }
}
