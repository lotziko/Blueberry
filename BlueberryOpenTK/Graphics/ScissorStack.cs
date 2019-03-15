using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace Blueberry
{
    public static partial class ScissorStack
    {
        private static Vec2 tmp;
        private static Stack<Rect> scissors = new Stack<Rect>();

        public static bool PushScissors(Rect scissor)
        {
            if (scissors.Count > 0)
            {
                // merge scissors
                /*var parent = scissors.Peek();
                var minX = (int)Math.Max(parent.X, scissor.X);
                var maxX = (int)Math.Min(parent.X + parent.Width, scissor.X + scissor.Width);
                if (maxX - minX < 1)
                    return false;

                var minY = (int)Math.Max(parent.Y, scissor.Y);
                var maxY = (int)Math.Min(parent.Y + parent.Height, scissor.Y + scissor.Height);
                if (maxY - minY < 1)
                    return false;

                scissor.X = minX;
                scissor.Y = minY;
                scissor.Width = maxX - minX;
                scissor.Height = (int)Math.Max(1, maxY - minY);*/
            }
            else
            {
                //GL.Enable(EnableCap.ScissorTest);
            }

            //flip coordinate system for framebuffer
            /*if (Graphics.IsDrawingToFramebuffer)
            {
                scissor.Y = Screen.Height - scissor.Y - scissor.Height;
            }*/

            scissors.Push(scissor);
            //GL.Scissor((int)scissor.X, (int)scissor.Y, (int)scissor.Width, (int)scissor.Height);

            return true;
        }

        public static Rect PopScissors()
        {
            var scissor = scissors.Pop();

            // reset the ScissorRectangle to the viewport bounds
            if (scissors.Count == 0)
            {
                //GL.Scissor(0, 0, Screen.Width, Screen.Height);
                //GL.Disable(EnableCap.ScissorTest);
            }
            else
            {
                var peek = scissors.Peek();
                //GL.Scissor((int)peek.X, (int)peek.Y, (int)peek.Width, (int)peek.Height);
            }

            return scissor;
        }

        /*public static Rect CalculateScissors(Camera camera, float viewportX, float viewportY, float viewportWidth, float viewportHeight, Mat batchTransform, Rect area)
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
        }*/
    }
}
