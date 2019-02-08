using Microsoft.Xna.Framework;

namespace BlueberryCore
{
    public static class PointExt
    {
        public static void Set(this ref Point p, int x, int y)
        {
            p.X = x;
            p.Y = y;
        }
    }
}
