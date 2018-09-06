using System;
using BlueberryCore.TextureAtlases;
using Microsoft.Xna.Framework;

namespace BlueberryCore.UI
{
    public class TiledDrawable : IDrawable
    {
        public float LeftWidth { get { return 0; } set { } }
        public float RightWidth { get { return 0; } set { } }
        public float TopHeight { get { return 0; } set { } }
        public float BottomHeight { get { return 0; } set { } }
        public float MinWidth { get { return _width; } set { } }
        public float MinHeight { get { return _height; } set { } }

        private TextureRegion _region;
        private int _width, _height;

        public void Draw(Graphics graphics, float x, float y, float width, float height, Color color)
        {
            for (int i = (int) x; i < x + width; i += _width)
            {
                for (int j = (int) y; j < y + height; j += _height)
                {
                    if (i + _width > x + width || j + _height > y + height)
                        _region.DrawClipped(graphics, i, j, Math.Min(x + width - i, _width), Math.Min(y + height - j, _height));
                    else
                        _region.Draw(graphics, i, j);
                }
            }
        }

        public TiledDrawable(TextureRegion region)
        {
            _region = region;
            _width = region.GetWidth();
            _height = region.GetHeight();
        }

        public TiledDrawable(string atlasName, string name)
        {
            var atlas = Core.content.Load<TextureAtlas>(atlasName);
            if (atlas == null)
                throw new ArgumentException("Wrong atlas name");
            var reg = atlas.FindRegion(name);

            _region = reg ?? throw new ArgumentException("Wrong region name");

            _width = reg.GetWidth();
            _height = reg.GetHeight();
        }

        public void SetPadding(float top, float bottom, float left, float right)
        {
            throw new NotImplementedException();
        }
    }
}
