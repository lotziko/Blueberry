using Blueberry.Monogame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Blueberry
{
    public partial class Graphics
    {
        private static Color col;

        protected TextureBatch tBatch;
        protected PrimitiveBatch pBatch;
        protected GraphicsDevice gDevice;
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
                }
            }
        }

        public static int CurrentTargetWidth => cTarget == null ? Screen.Width : cTarget.Width;
        public static int CurrentTargetHeigth => cTarget == null ? Screen.Height : cTarget.Height;

        private Mat transform, projection = new Mat(Matrix.Identity), previousProjection;

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

        public Mat Projection
        {
            get => projection;
            set
            {
                projection = value;
                pBatch.Projection = projection;
                tBatch.Projection = projection;
            }
        }


        public Graphics(GraphicsDevice gDevice)
        {
            this.gDevice = gDevice;
            tBatch = new TextureBatch(gDevice);
            pBatch = new PrimitiveBatch(gDevice);

            ActiveBatch = tBatch;
        }

        #region Scissors

        public bool PushScissors(Rect scissor)
        {
            throw new NotImplementedException();
        }

        public Rect PopScissors()
        {
            throw new NotImplementedException();
        }

        public static Rect CalculateScissors(Camera camera, float viewportX, float viewportY, float viewportWidth, float viewportHeight, Mat batchTransform, Rect area)
        {
            throw new NotImplementedException();
        }

    #endregion

        public void Flush()
        {
            activeBatch.Flush();
        }

        public void Begin()
        {
            activeBatch.Begin();
        }

        public void End()
        {
            activeBatch.End();
        }

        public void Clear(Col c)
        {
            gDevice.Clear(c.c);
        }

        public void Clear(Color c)
        {
            gDevice.Clear(c);
        }

        public void SetRenderTarget(RenderTarget target)
        {
            activeBatch.Flush();
            gDevice.SetRenderTarget(target);
            cTarget = target;
        }

        public void SetRenderTarget(RenderTarget2D target)
        {
            activeBatch.Flush();
            gDevice.SetRenderTarget(target);
            cTarget = target;
        }

        public void ResetRenderTarget()
        {
            activeBatch.Flush();
            gDevice.SetRenderTarget(null);
            cTarget = null;
        }

        #region Texture2D

        public void DrawTexture(Texture2D texture, float x, float y, Col? color = null)
        {
            col = (color ?? Col.White).c;
            ActiveBatch = tBatch;
            tBatch.Draw(texture, x, y, col);
        }

        public void DrawTexture(Texture2D texture, float x, float y, float width, float height, float u, float v, float u2, float v2, Col? color = null)
        {
            col = (color ?? Col.White).c;
            ActiveBatch = tBatch;
            tBatch.Draw(texture, x, y, width, height, u, v, u2, v2, col);
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
            tBatch.Draw(texture, x, y, width, height, 0, 0, width / texture.Width, height / texture.Height, col);
        }

        #endregion

        public void DrawRectangle(float x, float y, float width, float height, bool border = false, Col? color = null)
        {
            ActiveBatch = pBatch;
            col = (color ?? Col.White).c;
            if (border)
            {
                pBatch.Type = PrimitiveType.LineStrip;
                pBatch.AddVertex(x, y, col);
                pBatch.AddVertex(x + width, y, col);
                pBatch.AddVertex(x + width, y + height, col);
                pBatch.AddVertex(x, y + height, col);
                pBatch.AddVertex(x, y, col);
            }
            else
            {
                pBatch.Type = PrimitiveType.TriangleStrip;
                pBatch.AddVertex(x, y, col);
                pBatch.AddVertex(x + width, y, col);
                pBatch.AddVertex(x + width, y + height, col);
                pBatch.AddVertex(x, y + height, col);
            }
            pBatch.Flush();
        }
    }
}
