using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace BlueberryCore
{
    public partial class Graphics
    {
        public static Graphics instance;
        public static GraphicsDevice graphicsDevice => Core.graphicsDevice;

        protected static Vector2 bufferVector = Vector2.Zero;
        protected static Color col;

        protected readonly SpriteBatch spriteBatch;
        protected readonly PrimitiveBatch primitiveBatch;
        protected readonly GraphicsDevice gDevice;
        protected readonly Stack<RenderTargetBinding[]> tmpTargets;
        protected bool hasBegun, spriteMode;

        public bool HasBegun => hasBegun;

        /// <summary>
        /// P.S. Set matrix after render target
        /// </summary>
        public Matrix TransformMatrix
        {
            get => spriteBatch.TransformMatrix;
            set
            {
                spriteBatch.TransformMatrix = value;
                primitiveBatch.TransformMatrix = value;
            }
        }

        public Effect Shader
        {
            set
            {
                if (value == null)
                    spriteBatch.ResetShader();
                else
                    spriteBatch.SetShader(value);
                spriteBatch.Flush();
            }
        }

        public BlendState BlendState
        {
            set
            {
                spriteBatch.SetBlendState(value);
                spriteBatch.Flush();
            }
        }

        /*public void Draw(Texture2D texture, Vector2 position)
        {
            if (hasBegun)
            {
                if (!spriteBatch.HasBegun)
                    spriteBatch.Begin();
                spriteBatch.Draw(texture, position, Color.White);
            }
        }*/

        //public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color? color = null)
        //{
        //    if (hasBegun)
        //    {
        //        if (!spriteMode)
        //        {
        //            spriteBatch.Begin();
        //            spriteMode = true;
        //        }
        //        if (!spriteBatch.HasBegun)
        //            spriteBatch.Begin();
        //        spriteBatch.DrawString(spriteFont, text, position, color ?? Color.White);
        //    }
        //}

        public void Draw(Texture2D texture, Vector2 position, Rectangle? source = null, Color? color = null, Vector2? origin = null, Rectangle? destination = null, bool flipX = false, bool flipY = false)
        {
            if (hasBegun)
            {
                if (!spriteMode)
                {
                    spriteBatch.Begin();
                    spriteMode = true;
                }
                if (!spriteBatch.HasBegun)
                    spriteBatch.Begin();

                col = color ?? Color.White;
                col = Color.FromNonPremultiplied(col.R, col.G, col.B, col.A);

                if (destination.HasValue)
                    spriteBatch.Draw(texture, destination.Value, source, col, 0, origin ?? Vector2.Zero, (flipX ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flipY ? SpriteEffects.FlipVertically : SpriteEffects.None), 0);
                else
                    spriteBatch.Draw(texture, position, source, col, 0, origin ?? Vector2.Zero, 1, (flipX ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flipY ? SpriteEffects.FlipVertically : SpriteEffects.None), 0);
            }
        }

        public void DrawNonPremultiplied(Texture2D texture, Vector2 position, Rectangle? source = null, Color? color = null, Vector2? origin = null, Rectangle? destination = null, bool flipX = false, bool flipY = false)
        {
            if (hasBegun)
            {
                if (!spriteMode)
                {
                    spriteBatch.Begin();
                    spriteMode = true;
                }
                if (!spriteBatch.HasBegun)
                    spriteBatch.Begin();

                col = color ?? Color.White;

                if (destination.HasValue)
                    spriteBatch.Draw(texture, destination.Value, source, col, 0, origin ?? Vector2.Zero, (flipX ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flipY ? SpriteEffects.FlipVertically : SpriteEffects.None), 0);
                else
                    spriteBatch.Draw(texture, position, source, col, 0, origin ?? Vector2.Zero, 1, (flipX ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flipY ? SpriteEffects.FlipVertically : SpriteEffects.None), 0);
            }
        }

        public void DrawPrimitiveBegin(PrimitiveType type)
        {
            if (hasBegun)
            {
                if (spriteMode)
                {
                    if (spriteBatch.HasBegun)
                        spriteBatch.End();
                    spriteMode = false;
                }
                primitiveBatch.Begin(type);
            }
        }

        public void DrawVertex(float x, float y, Color? color = null)
        {
            primitiveBatch.AddVertex(x, y, color ?? Color.White);
        }

        public void DrawPrimitiveEnd()
        {
            primitiveBatch.End();
        }

        public void DrawRectangle(float x, float y, float width, float height, Color? color = null)
        {
            if (hasBegun)
            {
                if (spriteMode)
                {
                    if (spriteBatch.HasBegun)
                        spriteBatch.End();
                    spriteMode = false;
                }
                //if (spriteBatch.HasBegun)
                  //  spriteBatch.End();
                primitiveBatch.DrawRectangle(x, y, width, height, color ?? Color.White);
            }
        }

        public void DrawRectangleBorder(float x, float y, float width, float height, Color? color = null)
        {
            if (hasBegun)
            {
                if (spriteMode)
                {
                    if (spriteBatch.HasBegun)
                        spriteBatch.End();
                    spriteMode = false;
                }
                primitiveBatch.DrawRectangleBorder(x, y, width - 1, height - 1, color ?? Color.White);
            }
        }

        public void DrawLine(float x1, float y1, float x2, float y2, Color? color = null)
        {
            if (hasBegun)
            {
                if (spriteMode)
                {
                    if (spriteBatch.HasBegun)
                        spriteBatch.End();
                    spriteMode = false;
                }
                primitiveBatch.DrawLine(x1, y1, x2, y2, color ?? Color.White);
            }
        }

        public void SetRenderTarget(RenderTarget2D target)
        {
            StopDrawing();
            tmpTargets.Push(gDevice.GetRenderTargets());
            gDevice.SetRenderTarget(target);
            primitiveBatch.RefreshProjection();
        }

        public void ResetRenderTarget()
        {
            StopDrawing();
            gDevice.SetRenderTargets(tmpTargets.Pop());
            primitiveBatch.RefreshProjection();
        }

        public void Clear(Color color)
        {
            gDevice.Clear(color);
        }

        public void SetScissorTest(bool state)
        {
            spriteBatch.SetScissorTest(state);
            primitiveBatch.SetScissorTest(state);
        }

        protected void StopDrawing()
        {
            if (spriteMode)
                if (spriteBatch.HasBegun)
                    spriteBatch.End();
                else
                if (primitiveBatch.HasBegun)
                    primitiveBatch.End();
        }

        public void SetTransformMatrix(Matrix matrix)
        {
            spriteBatch.TransformMatrix = matrix;
            primitiveBatch.TransformMatrix = matrix;
        }

        public void Begin()
        {
            hasBegun = true;
            spriteMode = true;
        }

        public void Begin(Matrix transformMatrix)
        {
            TransformMatrix = transformMatrix;
            hasBegun = true;
            spriteMode = true;
        }

        public void End()
        {
            if (spriteBatch.HasBegun)
                spriteBatch.End();
            hasBegun = false;
        }

        public void Flush()
        {
            if (spriteMode)
                if (spriteBatch.HasBegun)
                {
                    spriteBatch.End();
                    spriteBatch.Begin();
                }
                else
                if (primitiveBatch.HasBegun)
                    if (primitiveBatch.HasBegun)
                    {
                        primitiveBatch.End();
                    }
        }

        public Graphics(GraphicsDevice graphicsDevice)
        {
            gDevice = graphicsDevice;
            spriteBatch = new SpriteBatch(graphicsDevice);
            primitiveBatch = new PrimitiveBatch(graphicsDevice);
            tmpTargets = new Stack<RenderTargetBinding[]>();
        }

    }
}
