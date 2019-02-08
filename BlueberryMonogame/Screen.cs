using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry
{
    public static partial class Screen
    {
        public static int Width { get; }
        public static int Height { get; }
        public static (float x, float y, float width, float height) Bounds { get; }
    }
}
