using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlueberryCore.UI
{
    public class TextureDrawable : IDrawable
    {
        public float LeftWidth { get; set; }
        public float RightWidth { get; set; }
        public float TopHeight { get; set; }
        public float BottomHeight { get; set; }
        public float MinWidth { get => _texture.Width; set { } }
        public float MinHeight { get => _texture.Height; set { } }

        private Texture2D _texture;
        private static Rectangle destination;

        public TextureDrawable(Texture2D texture)
        {
            _texture = texture;
        }

        public void Draw(Graphics graphics, float x, float y, float width, float height, Color color)
        {
            destination.Set(x, y, width, height);
            graphics.Draw(_texture, Vector2.Zero, null, color, null, destination);
        }

        public void SetPadding(float top, float bottom, float left, float right)
        {
            throw new NotImplementedException();
        }
    }
}
