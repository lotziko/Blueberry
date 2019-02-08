using System;
using BlueberryCore.TextureAtlases;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlueberryCore.UI
{
    public class TiledDrawable : IDrawable
    {
        public float LeftWidth { get { return 0; } set { } }
        public float RightWidth { get { return 0; } set { } }
        public float TopHeight { get { return 0; } set { } }
        public float BottomHeight { get { return 0; } set { } }
        public float MinWidth { get { return _bounds.Width; } set { } }
        public float MinHeight { get { return _bounds.Height; } set { } }

        private Texture2D _texture;
        private Rectangle _bounds, _tmpBounds;
        private Vector2 _pos;

        public void Draw(Graphics graphics, float x, float y, float width, float height, Color color)
        {
            for (int i = 0; i < width; i += _bounds.Width)
            {
                for (int j = 0; j < height; j += _bounds.Height)
                {
                    _pos.Set(x + i, y + j);
                    _tmpBounds.Width = (int)Math.Min(width - i, _bounds.Width);
                    _tmpBounds.Height = (int)Math.Min(height - j, _bounds.Height);
                    graphics.Draw(_texture, _pos, _tmpBounds, color);
                }
            }
        }

        public TiledDrawable(Texture2D texture)
        {
            _bounds = texture.Bounds;
            _tmpBounds = _bounds;
            _texture = texture;
        }

        public TiledDrawable(TextureRegion region)
        {
            _bounds = region.GetSource();
            _tmpBounds = _bounds;
            _texture = region.GetTexture();
        }

        public TiledDrawable(string atlasName, string name)
        {
            var atlas = Core.content.Load<TextureAtlas>(atlasName);
            if (atlas == null)
                throw new ArgumentException("Wrong atlas name");
            var reg = atlas.FindRegion(name) ?? throw new ArgumentException("Wrong region name");

            _texture = reg.GetTexture();
            _bounds = reg.GetSource();
            _tmpBounds = _bounds;
        }

        public void SetPadding(float top, float bottom, float left, float right)
        {
            throw new NotImplementedException();
        }
    }
}
