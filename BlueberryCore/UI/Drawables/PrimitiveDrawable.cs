using System;
using Microsoft.Xna.Framework;

namespace BlueberryCore.UI
{
    public class PrimitiveDrawable : IDrawable
    {
        public float LeftWidth { get; set; }
        public float RightWidth { get; set; }
        public float TopHeight { get; set; }
        public float BottomHeight { get; set; }
        public float MinWidth { get; set; }
        public float MinHeight { get; set; }

        private Color _color;
        private bool _border;

        public void Draw(Graphics graphics, float x, float y, float width, float height, Color color)
        {
            if (_border)
            {
                graphics.DrawRectangleBorder(x, y, width, height, new Color(_color, color.A));
            }
            else
            {
                graphics.DrawRectangle(x, y, width, height, new Color(_color, color.A));
            }
        }

        public void SetPadding(float top, float bottom, float left, float right)
        {
            throw new NotImplementedException();
        }

        public PrimitiveDrawable(Color color, bool border = false)
        {
            _color = color;
            _border = border;
        }
    }
}
