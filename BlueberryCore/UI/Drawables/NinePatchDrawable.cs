using BlueberryCore.TextureAtlases;
using Microsoft.Xna.Framework;
using System;

namespace BlueberryCore.UI
{
    public class NinePatchDrawable : IDrawable
    {
        public float LeftWidth { get { return ninePatch.padLeft != -1 ? ninePatch.padLeft : ninePatch.leftWidth; } set { } }
        public float RightWidth { get { return ninePatch.padRight != -1 ? ninePatch.padRight : ninePatch.rightWidth; } set { } }
        public float TopHeight { get { return ninePatch.padTop != -1 ? ninePatch.padTop : ninePatch.topHeight; } set { } }
        public float BottomHeight { get { return ninePatch.padBottom != -1 ? ninePatch.padBottom : ninePatch.bottomHeight; } set { } }
        public float MinWidth { get { return ninePatch.leftWidth + ninePatch.rightWidth + ninePatch.middleWidth; } set { } }
        public float MinHeight { get { return ninePatch.topHeight + ninePatch.bottomHeight + ninePatch.middleHeight; } set { } }

        private NinePatch ninePatch;

        public void Draw(Graphics graphics, float x, float y, float width, float height, Color color)
        {
            ninePatch.Draw(graphics, x, y, width, height, color);
        }

        public void SetPadding(float top, float bottom, float left, float right)
        {
            
        }

        public NinePatchDrawable(string atlasName, string name)
        {
            var atlas = Core.content.Load<TextureAtlas>(atlasName);
            if (atlas == null)
                throw new ArgumentException("Wrong atlas name");
            var patch = atlas.FindNinePatch(name);

            ninePatch = patch ?? throw new ArgumentException("Wrong ninepatch name");
        }

        public NinePatchDrawable(NinePatch ninePatch)
        {
            this.ninePatch = ninePatch;
        }
    }
}
