using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueberryCore.TextureAtlases;
using Microsoft.Xna.Framework;

namespace BlueberryCore.UI
{
    public class TextureRegionDrawable : IDrawable
    {
        public float LeftWidth { get; set; }
        public float RightWidth { get; set; }
        public float TopHeight { get; set; }
        public float BottomHeight { get; set; }
        public float MinWidth { get { return _region.GetWidth(); } set { } }
        public float MinHeight { get { return _region.GetHeight(); } set { } }

        private TextureRegion _region;

        public void Draw(Graphics graphics, float x, float y, float width, float height, Color color)
        {
            _region.Draw(graphics, x, y, width, height, color);
        }

        public void SetPadding(float top, float bottom, float left, float right)
        {
            throw new NotImplementedException();
        }

        public TextureRegionDrawable(string atlasName, string name)
        {
            var atlas = Core.content.Load<TextureAtlas>(atlasName);
            if (atlas == null)
                throw new ArgumentException("Wrong atlas name");
            var reg = atlas.FindRegion(name);

            _region = reg ?? throw new ArgumentException("Wrong region name");
        }

        public TextureRegionDrawable(TextureRegion region)
        {
            _region = region;
        }
    }
}
