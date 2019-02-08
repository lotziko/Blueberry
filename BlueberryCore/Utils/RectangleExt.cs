using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore
{
    public static class RectangleExt
    {
        public static Rectangle Set(this ref Rectangle r, float x, float y, float width, float height)
        {
            r.X = Convert.ToInt32(x);
            r.Y = Convert.ToInt32(y);
            r.Width = Convert.ToInt32(width);
            r.Height = Convert.ToInt32(height);
            return r;
        }

        public static Rectangle Set(this ref Rectangle r, int x, int y, int width, int height)
        {
            r.X = x;
            r.Y = y;
            r.Width = width;
            r.Height = height;
            return r;
        }
    }
}
