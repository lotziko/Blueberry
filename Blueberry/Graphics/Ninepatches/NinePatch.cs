using System;

namespace Blueberry
{
    public class NinePatch
    {
        private const int TOP_LEFT = 0, TOP_CENTER = 1, TOP_RIGHT = 2, MIDDLE_LEFT = 3, MIDDLE_CENTER = 4, MIDDLE_RIGHT = 5, BOTTOM_LEFT = 6, BOTTOM_CENTER = 7, BOTTOM_RIGHT = 8;
        private TextureRegion[] patches;

        public float MiddleWidth { get; private set; }
        public float MiddleHeight { get; private set; }
        public float LeftWidth { get; private set; }
        public float RightWidth { get; private set; }
        public float TopHeight { get; private set; }
        public float BottomHeight { get; private set; }
        public float PadLeft { get; private set; } = -1;
        public float PadRight { get; private set; } = -1;
        public float PadTop { get; private set; } = -1;
        public float PadBottom { get; private set; } = -1;

        internal void SetPads(float left, float right, float top, float bottom)
        {
            PadLeft = left;
            PadRight = right;
            PadTop = top;
            PadBottom = bottom;
        }

        public NinePatch(TextureRegion region, int left, int right, int top, int bottom)
        {
            if (region == null)
            {
                throw new Exception("Region can't be null");
            }
            int MiddleWidth = region.Width - left - right;
            int MiddleHeight = region.Height - top - bottom;

            var patches = new TextureRegion[9];
            if (top > 0)
            {
                if (left > 0) patches[TOP_LEFT] = new TextureRegion(region, 0, 0, left, top);
                if (MiddleWidth > 0) patches[TOP_CENTER] = new TextureRegion(region, left, 0, MiddleWidth, top);
                if (right > 0) patches[TOP_RIGHT] = new TextureRegion(region, left + MiddleWidth, 0, right, top);
            }
            if (MiddleHeight > 0)
            {
                if (left > 0) patches[MIDDLE_LEFT] = new TextureRegion(region, 0, top, left, MiddleHeight);
                if (MiddleWidth > 0) patches[MIDDLE_CENTER] = new TextureRegion(region, left, top, MiddleWidth, MiddleHeight);
                if (right > 0) patches[MIDDLE_RIGHT] = new TextureRegion(region, left + MiddleWidth, top, right, MiddleHeight);
            }
            if (bottom > 0)
            {
                if (left > 0) patches[BOTTOM_LEFT] = new TextureRegion(region, 0, top + MiddleHeight, left, bottom);
                if (MiddleWidth > 0) patches[BOTTOM_CENTER] = new TextureRegion(region, left, top + MiddleHeight, MiddleWidth, bottom);
                if (right > 0) patches[BOTTOM_RIGHT] = new TextureRegion(region, left + MiddleWidth, top + MiddleHeight, right, bottom);
            }

            // If split only vertical, move splits from right to center.
            if (left == 0 && MiddleWidth == 0)
            {
                patches[TOP_CENTER] = patches[TOP_RIGHT];
                patches[MIDDLE_CENTER] = patches[MIDDLE_RIGHT];
                patches[BOTTOM_CENTER] = patches[BOTTOM_RIGHT];
                patches[TOP_RIGHT] = null;
                patches[MIDDLE_RIGHT] = null;
                patches[BOTTOM_RIGHT] = null;
            }
            // If split only horizontal, move splits from bottom to center.
            if (top == 0 && MiddleHeight == 0)
            {
                patches[MIDDLE_LEFT] = patches[BOTTOM_LEFT];
                patches[MIDDLE_CENTER] = patches[BOTTOM_CENTER];
                patches[MIDDLE_RIGHT] = patches[BOTTOM_RIGHT];
                patches[BOTTOM_LEFT] = null;
                patches[BOTTOM_CENTER] = null;
                patches[BOTTOM_RIGHT] = null;
            }

            Load(patches);
        }

        private void Load(TextureRegion[] patches)
        {
            this.patches = new TextureRegion[9];
            if (patches[BOTTOM_LEFT] != null)
            {
                this.patches[BOTTOM_LEFT] = patches[BOTTOM_LEFT];
                LeftWidth = patches[BOTTOM_LEFT].Width;
                BottomHeight = patches[BOTTOM_LEFT].Height;
            }
            if (patches[BOTTOM_CENTER] != null)
            {
                this.patches[BOTTOM_CENTER] = patches[BOTTOM_CENTER];
                MiddleWidth = Math.Max(MiddleWidth, patches[BOTTOM_CENTER].Width);
                BottomHeight = Math.Max(BottomHeight, patches[BOTTOM_CENTER].Height);
            }
            if (patches[BOTTOM_RIGHT] != null)
            {
                this.patches[BOTTOM_RIGHT] = patches[BOTTOM_RIGHT];
                RightWidth = Math.Max(RightWidth, patches[BOTTOM_RIGHT].Width);
                BottomHeight = Math.Max(BottomHeight, patches[BOTTOM_RIGHT].Height);
            }
            if (patches[MIDDLE_LEFT] != null)
            {
                this.patches[MIDDLE_LEFT] = patches[MIDDLE_LEFT];
                LeftWidth = Math.Max(LeftWidth, patches[MIDDLE_LEFT].Width);
                MiddleHeight = Math.Max(MiddleHeight, patches[MIDDLE_LEFT].Height);
            }
            if (patches[MIDDLE_CENTER] != null)
            {
                this.patches[MIDDLE_CENTER] = patches[MIDDLE_CENTER];
                MiddleWidth = Math.Max(MiddleWidth, patches[MIDDLE_CENTER].Width);
                MiddleHeight = Math.Max(MiddleHeight, patches[MIDDLE_CENTER].Height);
            }
            if (patches[MIDDLE_RIGHT] != null)
            {
                this.patches[MIDDLE_RIGHT] = patches[MIDDLE_RIGHT];
                RightWidth = Math.Max(RightWidth, patches[MIDDLE_RIGHT].Width);
                MiddleHeight = Math.Max(MiddleHeight, patches[MIDDLE_RIGHT].Height);
            }
            if (patches[TOP_LEFT] != null)
            {
                this.patches[TOP_LEFT] = patches[TOP_LEFT];
                LeftWidth = Math.Max(LeftWidth, patches[TOP_LEFT].Width);
                TopHeight = Math.Max(TopHeight, patches[TOP_LEFT].Height);
            }
            if (patches[TOP_CENTER] != null)
            {
                this.patches[TOP_CENTER] = patches[TOP_CENTER];
                MiddleWidth = Math.Max(MiddleWidth, patches[TOP_CENTER].Width);
                TopHeight = Math.Max(TopHeight, patches[TOP_CENTER].Height);
            }
            if (patches[TOP_RIGHT] != null)
            {
                this.patches[TOP_RIGHT] = patches[TOP_RIGHT];
                RightWidth = Math.Max(RightWidth, patches[TOP_RIGHT].Width);
                TopHeight = Math.Max(TopHeight, patches[TOP_RIGHT].Height);
            }

        }

        public void Draw(Graphics graphics, float x, float y, float width, float height, Col? color = null)
        {
            var col = color ?? Col.White;

            float centerColumnX = x + LeftWidth;
            float rightColumnX = x + width - RightWidth;
            float middleRowY = y + TopHeight;
            float topRowY = y + height - BottomHeight;

            if (patches[TOP_LEFT] != null)
                patches[TOP_LEFT].Draw(graphics, x, y, centerColumnX - x, middleRowY - y, col);
            if (patches[TOP_CENTER] != null)
                patches[TOP_CENTER].Draw(graphics, centerColumnX, y, rightColumnX - centerColumnX, middleRowY - y, col);
            if (patches[TOP_RIGHT] != null)
                patches[TOP_RIGHT].Draw(graphics, rightColumnX, y, x + width - rightColumnX, middleRowY - y, col);

            if (patches[MIDDLE_LEFT] != null)
                patches[MIDDLE_LEFT].Draw(graphics, x, middleRowY, centerColumnX - x, topRowY - middleRowY, col);
            if (patches[MIDDLE_CENTER] != null)
                patches[MIDDLE_CENTER].Draw(graphics, centerColumnX, middleRowY, rightColumnX - centerColumnX, topRowY - middleRowY, col);
            if (patches[MIDDLE_RIGHT] != null)
                patches[MIDDLE_RIGHT].Draw(graphics, rightColumnX, middleRowY, x + width - rightColumnX, topRowY - middleRowY, col);

            if (patches[BOTTOM_LEFT] != null)
                patches[BOTTOM_LEFT].Draw(graphics, x, topRowY, centerColumnX - x, y + height - topRowY, col);
            if (patches[BOTTOM_CENTER] != null)
                patches[BOTTOM_CENTER].Draw(graphics, centerColumnX, topRowY, rightColumnX - centerColumnX, y + height - topRowY, col);
            if (patches[BOTTOM_RIGHT] != null)
                patches[BOTTOM_RIGHT].Draw(graphics, rightColumnX, topRowY, x + width - rightColumnX, y + height - topRowY, col);
        }
    }
}
