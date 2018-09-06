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
            r.X = (int) x;
            r.Y = (int) y;
            r.Width = (int) width;
            r.Height = (int) height;
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
