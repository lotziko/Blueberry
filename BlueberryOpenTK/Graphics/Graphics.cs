using BlueberryOpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace Blueberry
{
    public partial class Graphics
    {
        private static Color4 col;

        protected PrimitiveBatch pBatch = new PrimitiveBatch();
        protected TextureBatch tBatch = new TextureBatch();
        protected static RenderTarget2D cTarget;

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
                    //TODO исправить костыль
                    if (activeBatch == tBatch)
                    {
                        pBatch.Mode = OpenTK.Graphics.OpenGL4.PolygonMode.Fill;
                    }
                    //activeBatch.ResetAttributes();
                }
            }
        }

        public static int CurrentTargetWidth => cTarget == null ? Screen.Width : cTarget.Width;
        public static int CurrentTargetHeigth => cTarget == null ? Screen.Height : cTarget.Height;

        /// <summary>
        /// Is important to know because opengl framebuffer coordinate system starts from down to up
        /// </summary>
        public static bool IsDrawingToFramebuffer => cTarget == null;

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
            activeBatch.End();

            GL.Disable(EnableCap.Blend);
        }

        private Mat transform, projection = new Mat(OpenTK.Matrix4.Identity), previousProjection;

        public Mat Transform
        {
            get => transform;
            set
            {
                transform = value;
                pBatch.Transform = transform.m;
                tBatch.Transform = transform.m;
            }
        }

        public Mat Projection
        {
            get => projection;
            set
            {
                projection = value;
                pBatch.Projection = projection.m;
                tBatch.Projection = projection.m;
            }
        }

        public Graphics()
        {
            ActiveBatch = tBatch;
        }

        public void Clear(Col c)
        {
            GL.ClearColor(c.c.R, c.c.G, c.c.B, c.c.A);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public void SetRenderTarget(RenderTarget2D target)
        {
            activeBatch.Flush();
            RenderTarget2D.Bind(target);
            previousProjection = projection;
            Projection = target.DefaultProjection;
            cTarget = target;
        }

        public void ResetRenderTarget()
        {
            activeBatch.Flush();
            RenderTarget2D.Bind(null);
            Projection = previousProjection;
            cTarget = null;
        }

        public void DrawTexture(Texture2D texture, float x, float y, Col? color = null)
        {
            col = (color ?? Col.White).c;
            ActiveBatch = tBatch;
            tBatch.Draw(texture, x, y, col);
        }

        public void DrawTexture(Texture2D texture, Rect source, Rect destination, Col? color = null)
        {
            col = (color ?? Col.White).c;
            ActiveBatch = tBatch;
            tBatch.Draw(texture, destination.X, destination.Y, destination.Width, destination.Height, source.X * texture.TexelH, source.Y * texture.TexelV, source.Right * texture.TexelH, source.Bottom * texture.TexelV, col);
        }

        public void DrawTexture(Texture2D texture, float x, float y, float width, float height, float u, float v, float u2, float v2, Col? color = null)
        {
            col = (color ?? Col.White).c;
            ActiveBatch = tBatch;
            tBatch.Draw(texture, x, y, width, height, u, v, u2, v2, col);
        }

        public void DrawTexture(Texture2D texture, Rect destination, Col? color = null)
        {
            col = (color ?? Col.White).c;
            ActiveBatch = tBatch;
            tBatch.Draw(texture, destination.X, destination.Y, destination.Width, destination.Height, col);
        }

        public void DrawTexture(Texture2D texture, float x, float y, float width, float height, Col? color = null)
        {
            col = (color ?? Col.White).c;
            ActiveBatch = tBatch;
            tBatch.Draw(texture, x, y, width, height, col);
        }

        public void DrawTextureTiled(Texture2D texture, float x, float y, float width, float height, Col? color = null)
        {
            col = (color ?? Col.White).c;
            ActiveBatch = tBatch;
            tBatch.Draw(texture, x, y, width, height, 0, 0, 1 * texture.Width * texture.TexelH, 1 * texture.Height * texture.TexelV, col);
        }

        public void DrawRectangle(Rect rect, bool border = false, Col? color = null)
        {
            DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height, border, color);
        }

        public void DrawRectangle(float x, float y, float width, float height, bool border = false, Col? color = null)
        {
            ActiveBatch = pBatch;
            col = (color ?? Col.White).c;
            if (border)
            {
                //pBatch.Type = OpenTK.Graphics.OpenGL4.PrimitiveType.Patches;загугли
                pBatch.Type = PrimitiveType.Quads;
                pBatch.Mode = PolygonMode.Line;
                pBatch.AddVertex(x, y, col);
                pBatch.AddVertex(x + width, y, col);
                pBatch.AddVertex(x + width, y + height, col);
                pBatch.AddVertex(x, y + height, col);
            }
            else
            {
                pBatch.Type = PrimitiveType.Quads;
                pBatch.Mode = PolygonMode.Fill;
                pBatch.AddVertex(x, y, col);
                pBatch.AddVertex(x + width, y, col);
                pBatch.AddVertex(x + width, y + height, col);
                pBatch.AddVertex(x, y + height, col);
            }
        }
    }
}
