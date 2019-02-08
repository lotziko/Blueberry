using Microsoft.Xna.Framework;
using System;

namespace Blueberry
{
    public static partial class ScissorStack
    {
        private static Vec2 tmp;

        public static bool PushScissors(Rect scissor)
        {
            throw new NotImplementedException();
        }

        public static Rectangle PopScissors()
        {
            throw new NotImplementedException();
        }

        public static Rect CalculateScissors(Camera camera, float viewportX, float viewportY, float viewportWidth, float viewportHeight, Mat batchTransform, Rect area)
        {
            var scissor = new Rect();

            tmp.Set(area.X, area.Y);
            tmp = camera.Project(Vec2.Transform(tmp, batchTransform), viewportX, viewportY, viewportWidth, viewportHeight);

            scissor.X = tmp.X;
            scissor.Y = tmp.Y;

            tmp.Set(area.X + area.Width, area.Y + area.Height);
            tmp = camera.Project(Vec2.Transform(tmp, batchTransform), viewportX, viewportY, viewportWidth, viewportHeight);

            scissor.Width = tmp.X - scissor.X;
            scissor.Height = tmp.Y - scissor.Y;

            return scissor;
        }
    }
}
