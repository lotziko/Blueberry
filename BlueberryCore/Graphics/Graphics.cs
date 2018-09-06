using BlueberryCore.TextureAtlases;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlueberryCore
{
    public class Graphics
    {
        public static Graphics instance;
        public static GraphicsDevice graphicsDevice => Core.graphicsDevice;

        private static Vector2 bufferVector = Vector2.Zero;

        public readonly SpriteBatch spriteBatch;
        public readonly PrimitiveBatch primitiveBatch;
        private bool hasBegun;

        public bool HasBegun { get { return hasBegun; } }
        
        public Matrix TransformMatrix
        {
            get => spriteBatch.TransformMatrix;
            set
            {
                spriteBatch.TransformMatrix = value;
                primitiveBatch.TransformMatrix = value;
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

        public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color? color = null)
        {
            if (hasBegun)
            {
                if (!spriteBatch.HasBegun)
                    spriteBatch.Begin();
                spriteBatch.DrawString(spriteFont, text, position, color ?? Color.White);
            }
        }

        public void Draw(Texture2D texture, Vector2 position, Rectangle? source = null, Color? color = null, Vector2? origin = null, Rectangle? destination = null, bool flipX = false, bool flipY = false)
        {
            if (hasBegun)
            {
                if (!spriteBatch.HasBegun)
                    spriteBatch.Begin();

                var col = color ?? Color.White;
                float a = col.A / 255f;
                col.A = 255;
                col *= a;

                if (destination.HasValue)
                    spriteBatch.Draw(texture, destination.Value, source, col, 0, origin ?? Vector2.Zero, (flipX ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flipY ? SpriteEffects.FlipVertically : SpriteEffects.None), 0);
                else
                    spriteBatch.Draw(texture, position, source, col, 0, origin ?? Vector2.Zero, 1, (flipX ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flipY ? SpriteEffects.FlipVertically : SpriteEffects.None), 0);
            }
        }
        
        public void DrawRectangle(float x, float y, float width, float height, Color? color = null)
        {
            if (hasBegun)
            {
                if (spriteBatch.HasBegun)
                    spriteBatch.End();
                primitiveBatch.DrawRectangle(x, y, width, height, color ?? Color.White);
            }
        }

        public void DrawRectangleBorder(float x, float y, float width, float height, Color? color = null)
        {
            if (hasBegun)
            {
                if (spriteBatch.HasBegun)
                    spriteBatch.End();
                primitiveBatch.DrawRectangleBorder(x, y, width - 1, height - 1, color ?? Color.White);
            }
        }

        /// <summary>
        /// P.S. Set matrix after render target
        /// </summary>
        /// <param name="matrix"></param>
        public void SetTransformMatrix(Matrix matrix)
        {
            spriteBatch.TransformMatrix = matrix;
            primitiveBatch.TransformMatrix = matrix;
        }

        public void Begin()
        {
            hasBegun = true;
        }

        public void Begin(Matrix transformMatrix)
        {
            TransformMatrix = transformMatrix;
            hasBegun = true;
        }

        public void End()
        {
            if (spriteBatch.HasBegun)
                spriteBatch.End();
            hasBegun = false;
        }

        public void Flush()
        {
            if (spriteBatch.HasBegun)
            {
                spriteBatch.Flush();
            }
        }

        public Graphics(GraphicsDevice graphicsDevice)
        {
            spriteBatch = new SpriteBatch(graphicsDevice);
            primitiveBatch = new PrimitiveBatch(graphicsDevice);
        }

    }
}
