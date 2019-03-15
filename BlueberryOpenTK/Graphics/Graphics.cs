using Blueberry.OpenGL;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

namespace Blueberry
{
    public partial class Graphics
    {
        private static Color4 col;
        private Stack<Rect> scissors = new Stack<Rect>();
        private GraphicsDevice device;

        protected PrimitiveBatch pBatch;
        protected TextureBatch tBatch;

        protected IBatch activeBatch;

        protected IBatch ActiveBatch
        {
            get => activeBatch;
            set
            {
                if (activeBatch != value)
                {
                    activeBatch?.Flush();
                    activeBatch = value;
                }
            }
        }

        private Mat transform;

        public Mat Transform
        {
            get => transform;
            set
            {
                transform = value;
                pBatch.Transform = transform;
                tBatch.Transform = transform;
            }
        }

        public Graphics(GraphicsDevice gDevice)
        {
            device = gDevice;
            tBatch = new TextureBatch(device);
            pBatch = new PrimitiveBatch(device);

            ActiveBatch = tBatch;
        }

        #region Scissors

        public bool PushScissors(Rect scissor)
        {
            scissors.Push(scissor);
            device.ScissorRectangle = scissor;

            return true;
        }

        public Rect PopScissors()
        {
            var scissor = scissors.Pop();
            if (scissors.Count == 0)
                device.ScissorRectangle = null;
            else
                device.ScissorRectangle = scissors.Peek();
            return scissor;
        }

        public static Rect CalculateScissors(Camera camera, float viewportX, float viewportY, float viewportWidth, float viewportHeight, Mat batchTransform, Rect area)
        {
            var tmp = new Vec2(0, 0);
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

        #endregion


        public void Flush()
        {
            activeBatch.Flush();
        }

        public void Begin()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            activeBatch.Begin();
        }

        public void End()
        {
            activeBatch.Flush();

            GL.Disable(EnableCap.Blend);
        }

        public void Clear(Col c)
        {
            device.Clear(c.c);
        }

        public void SetRenderTarget(RenderTarget target)
        {
            activeBatch.Flush();
            device.SetRenderTarget(target);
        }

        public void ResetRenderTarget()
        {
            activeBatch.Flush();
            device.SetRenderTarget(null);
        }

        public void DrawTexture(Texture texture, float x, float y, Col? color = null)
        {
            col = (color ?? Col.White).c;
            ActiveBatch = tBatch;
            tBatch.Draw(texture, x, y, col);
        }

        public void DrawTexture(Texture texture, float x, float y, float width, float height, Col? color = null)
        {
            col = (color ?? Col.White).c;
            ActiveBatch = tBatch;
            tBatch.Draw(texture, x, y, width, height, col);
        }

        public void DrawTexture(Texture texture, float x, float y, float width, float height, float u, float v, float u2, float v2, Col? color = null)
        {
            col = (color ?? Col.White).c;
            ActiveBatch = tBatch;
            tBatch.Draw(texture, x, y, width, height, u, v, u2, v2, col);
        }

        public void DrawTextureTiled(Texture texture, float x, float y, float width, float height, Col? color = null)
        {
            col = (color ?? Col.White).c;
            ActiveBatch = tBatch;
            tBatch.Draw(texture, x, y, width, height, 0, 0, 1 * width * texture.TexelH, 1 * height * texture.TexelV, col);
        }

        public void DrawRectangle(Rect rect, bool border = false, Col? color = null)
        {
            DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height, border, color);
        }

        public void DrawRectangle(float x, float y, float width, float height, bool border = false, Col? color = null)
        {
            ActiveBatch = pBatch;
            col = (color ?? Col.White).c;
            pBatch.DrawRectangle(x, y, width, height, col, border);
        }
    }
}
