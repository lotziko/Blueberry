using Blueberry.OpenGL;

namespace Blueberry
{
    public partial class TextureRegion
    {
        protected Texture texture;

        public Texture Texture => texture;

        public virtual void Draw(Graphics graphics, float x, float y, Col? col = null)
        {
            graphics.DrawTexture(texture, x, y, regionWidth, regionHeight, u, v, u2, v2, col);
        }

        public virtual void Draw(Graphics graphics, float x, float y, float width, float height, Col? col = null)
        {
            graphics.DrawTexture(texture, x, y, width, height, u, v, u2, v2, col);
        }

        public TextureRegion(Texture texture, int x, int y, int width, int height)
        {
            this.texture = texture;
            this.x = x;
            this.y = y;
            regionWidth = width;
            regionHeight = height;

            u = x * texture.TexelH;
            v = y * texture.TexelV;

            u2 = (x + width) * texture.TexelH;
            v2 = (y + height) * texture.TexelV;

            if (regionWidth == 1 && regionHeight == 1)
            {
                float adjustX = 0.25f / texture.Width;
                u += adjustX;
                u2 -= adjustX;
                float adjustY = 0.25f / texture.Height;
                v += adjustY;
                v2 -= adjustY;
            }
        }

        public TextureRegion(TextureRegion region, int left, int top, int width, int height)
        {
            texture = region.texture;
            u = region.u + left * texture.TexelH;
            v = region.v + top * texture.TexelV;
            u2 = u + width * texture.TexelH;
            v2 = v + height * texture.TexelV;

            if (regionWidth == 1 && regionHeight == 1)
            {
                float adjustX = 0.25f / texture.Width;
                u += adjustX;
                u2 -= adjustX;
                float adjustY = 0.25f / texture.Height;
                v += adjustY;
                v2 -= adjustY;
            }
        }
    }
}
