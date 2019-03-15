using Blueberry.OpenGL;
using System;

namespace Blueberry.UI
{
    public class NinePatchDrawable : IDrawable
    {
        public float LeftWidth { get { return ninePatch.PadLeft != -1 ? ninePatch.PadLeft : ninePatch.LeftWidth; } set { } }
        public float RightWidth { get { return ninePatch.PadRight != -1 ? ninePatch.PadRight : ninePatch.RightWidth; } set { } }
        public float TopHeight { get { return ninePatch.PadTop != -1 ? ninePatch.PadTop : ninePatch.TopHeight; } set { } }
        public float BottomHeight { get { return ninePatch.PadBottom != -1 ? ninePatch.PadBottom : ninePatch.BottomHeight; } set { } }
        public float MinWidth { get { return ninePatch.LeftWidth + ninePatch.RightWidth + ninePatch.MiddleWidth; } set { } }
        public float MinHeight { get { return ninePatch.TopHeight + ninePatch.BottomHeight + ninePatch.MiddleHeight; } set { } }

        private NinePatch ninePatch;

        public NinePatchDrawable(NinePatch ninePatch)
        {
            this.ninePatch = ninePatch;
        }

        public void Draw(Graphics graphics, float x, float y, float width, float height, Col color)
        {
            ninePatch.Draw(graphics, x, y, width, height, color);
        }

        public void SetPadding(float top, float bottom, float left, float right)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return ninePatch.ToString();
        }
    }
}
