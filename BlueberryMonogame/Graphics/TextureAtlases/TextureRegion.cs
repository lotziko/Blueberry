using Microsoft.Xna.Framework.Graphics;

namespace Blueberry
{
    public partial class TextureRegion
    {
        protected Texture2D texture;

        public Texture2D Texture => texture;


        public void Draw(Graphics graphics, float x, float y, Col? col = null)
        {
            graphics.DrawTexture(texture, x, y, col);
        }

        public void Draw(Graphics graphics, float x, float y, float width, float height, Col? col = null)
        {
            graphics.DrawTexture(texture, x, y, width, height, col);
        }

        public TextureRegion(Texture2D texture, int x, int y, int width, int height)
        {
            this.texture = texture;
            this.x = x;
            this.y = y;
            regionWidth = width;
            regionHeight = height;

            float texelH = 1.0f / texture.Width, texelV = 1.0f / texture.Height;

            u = x * texelH;
            v = y * texelV;

            u2 = (x + width) * texelH;
            v2 = (y + height) * texelV;
        }

        public TextureRegion(Texture texture, int x, int y, int width, int height)
        {
            this.texture = texture;
            this.x = x;
            this.y = y;
            regionWidth = width;
            regionHeight = height;

            float texelH = 1.0f / texture.Width, texelV = 1.0f / texture.Height;

            u = x * texelH;
            v = y * texelV;

            u2 = (x + width) * texelH;
            v2 = (y + height) * texelV;
        }

        public TextureRegion(TextureRegion region, int left, int top, int width, int height)
        {
            throw new System.NotImplementedException();
        }
    }
}
