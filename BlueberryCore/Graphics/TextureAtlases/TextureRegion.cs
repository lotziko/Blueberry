using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BlueberryCore
{
    public class TextureRegion
    {
        private Texture2D texture;
        private Rectangle source;
        private Rectangle destination = new Rectangle();
        private Vector2 offset = Vector2.Zero;
        private static Vector2 position = new Vector2();
        private static Rectangle tmpSrc = new Rectangle();

        public Rectangle GetSource() => source;

        public int GetWidth() => source.Width;
        public int GetHeight() => source.Height;
        public Texture2D GetTexture() => texture;

        public void Draw(SpriteBatch batch, float x, float y)
        {
            position.X = x;
            position.Y = y;
            batch.Draw(texture, position + offset, source, Color.White);
        }

        public void Draw(Graphics graphics, float x, float y)
        {
            position.X = x;
            position.Y = y;
            graphics.Draw(texture, position + offset, source, Color.White);
        }

        public void Draw(Graphics graphics, Vector2 position)
        {
            graphics.Draw(texture, position + offset, source, Color.White);
        }

        public void Draw(Graphics graphics, float x, float y, bool flipX, bool flipY)
        {
            position.X = x;
            position.Y = y;
            graphics.Draw(texture, position + offset, source, Color.White, null, null, flipX, flipY);//texture, position, source, Color.White, 0, Vector2.Zero, 1, (flipX ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flipY ? SpriteEffects.FlipVertically : SpriteEffects.None), 0);
        }
        
        public void Draw(Graphics graphics, float x, float y, bool flipX, bool flipY, Vector2 origin, Color? color = null)
        {
            position.X = x;
            position.Y = y;
            if (flipX)
            {
                origin.X = source.Width - origin.X;
            }
            if (flipY)
            {
                origin.Y = source.Height - origin.Y;
            }
            graphics.Draw(texture, position + offset, source, color, origin, null, flipX, flipY);
            //batch.Draw(texture, position, source, color, 0, origin, 1, (flipX ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flipY ? SpriteEffects.FlipVertically : SpriteEffects.None), 0);
        }

        public void Draw(Graphics graphics, float x, float y, float width, float height, Color? color = null)
        {
            position.X = x;
            position.Y = y;
            destination.X = (int) x;
            destination.Y = (int) y;
            destination.Width = (int) width;
            destination.Height = (int) height;
            graphics.Draw(texture, position + offset, source, color, null, destination, false, false);
        }

        /// <summary>
        /// Draws texture clipped
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width">Width amount to cut from source bounds</param>
        /// <param name="height">Height amount to cut from source bounds</param>

        public void DrawClipped(Graphics graphics, float x, float y, float width, float height)
        {
            if (width > source.Width || height > source.Height)
                throw new Exception("width/height can't be bigger than actual region");

            position.X = x;
            position.Y = y;
            tmpSrc.X = source.X;
            tmpSrc.Y = source.Y;
            tmpSrc.Width = (int) width;
            tmpSrc.Height = (int) height;
            graphics.Draw(texture, position + offset, tmpSrc);
        }

        public void DrawTiled(Graphics graphics, float x, float y, float width, float height, Color? color = null)
        {
            position.X = x;
            position.Y = y;
            destination.X = (int)x;
            destination.Y = (int)y;
            destination.Width = (int)width;
            destination.Height = (int)height;

            
            if (ScissorStack.PushScissors(new Rectangle((int)x, (int)y, (int)width, (int)height)))
            {
                position.X = x;
                position.Y = y;
                graphics.SetScissorTest(true);
                for(int i = 0; i < width; i += source.Width)
                {
                    for(int j = 0; j < height; j += source.Height)
                    {
                        graphics.Draw(texture, position + offset, source, color);
                        position.Y += source.Height;
                    }
                    position.Y = y;
                    position.X += source.Width;
                }

                graphics.SetScissorTest(false);
                ScissorStack.PopScissors();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>

        public TextureRegion(Texture2D texture, int left, int top, int width, int height, int offsetX = 0, int offsetY = 0)
        {
            this.texture = texture;
            source = new Rectangle(left, top, width, height);
            offset = new Vector2(offsetX, offsetY);
        }

        public TextureRegion(TextureRegion region, int left, int top, int width, int height)
        {
            texture = region.texture;
            Rectangle oldBounds = region.source;
            int x = oldBounds.X + left;
            int y = oldBounds.Y + top;
            source = tmpSrc = new Rectangle(x, y, width, height);
        }
    }
}
