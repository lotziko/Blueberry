
using Microsoft.Xna.Framework.Graphics;

namespace Blueberry
{
    public partial class TextureRegion
    {
        private Texture2D texture;

        public Texture2D Texture => texture;

        public void Draw(Graphics graphics, float x, float y, Col? col = null)
        {
            destination.Set(x, y, source.Width, source.Height);
            throw new System.NotImplementedException();
            //graphics.DrawTexture(texture, source, destination, col);
        }

        public void Draw(Graphics graphics, float x, float y, float width, float height, Col? col = null)
        {
            destination.Set(x, y, width, height);
            throw new System.NotImplementedException();
            //graphics.DrawTexture(texture, source, destination, col);
        }

        public TextureRegion(Texture2D texture, int left, int top, int width, int height, int offsetX = 0, int offsetY = 0)
        {
            this.texture = texture;
            source = new Rect(left, top, width, height);
            offset = new Vec2(offsetX, offsetY);
        }

        public TextureRegion(TextureRegion region, int left, int top, int width, int height)
        {
            texture = region.texture;
            Rect oldBounds = region.source;
            var x = oldBounds.X + left;
            var y = oldBounds.Y + top;
            source = tmpSrc = new Rect(x, y, width, height);
        }
    }
}
