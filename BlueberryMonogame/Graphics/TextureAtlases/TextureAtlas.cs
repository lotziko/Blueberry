using Microsoft.Xna.Framework.Graphics;

namespace Blueberry
{
    public partial class TextureAtlas
    {
        internal Texture2D[] texture;

        private TextureRegion CreateRegion(int page, int left, int top, int width, int height, int offsetX = 0, int offsetY = 0)
        {
            return new TextureRegion(texture[page], left, top, width, height, offsetX, offsetY);
        }
    }
}
