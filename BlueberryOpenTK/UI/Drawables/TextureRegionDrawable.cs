using Blueberry.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blueberry.UI
{
    public class TextureRegionDrawable : IDrawable
    {
        public float LeftWidth { get => 0; set => throw new NotImplementedException(); }
        public float RightWidth { get => 0; set => throw new NotImplementedException(); }
        public float TopHeight { get => 0; set => throw new NotImplementedException(); }
        public float BottomHeight { get => 0; set => throw new NotImplementedException(); }
        public float MinWidth { get => region.Width; set => throw new NotImplementedException(); }
        public float MinHeight { get => region.Height; set => throw new NotImplementedException(); }

        private TextureRegion region;

        public TextureRegionDrawable(TextureRegion region)
        {
            this.region = region;
        }

        public void Draw(Graphics graphics, float x, float y, float width, float height, Col color)
        {
            region.Draw(graphics, x, y, width, height, color);
        }

        public void SetPadding(float top, float bottom, float left, float right)
        {
            throw new NotImplementedException();
        }
    }
}
