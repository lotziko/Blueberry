using BlueberryCore.TextureAtlases;
using Microsoft.Xna.Framework;
using System;

namespace BlueberryCore
{
    public class NinePatch
    {
        private const int TOP_LEFT = 0, TOP_CENTER = 1, TOP_RIGHT = 2, MIDDLE_LEFT = 3, MIDDLE_CENTER = 4, MIDDLE_RIGHT = 5, BOTTOM_LEFT = 6, BOTTOM_CENTER = 7, BOTTOM_RIGHT = 8;
        private TextureRegion[] _patches;

        public float middleWidth { get; private set; }
        public float middleHeight { get; private set; }
        public float leftWidth { get; private set; }
        public float rightWidth { get; private set; }
        public float topHeight { get; private set; }
        public float bottomHeight { get; private set; }
        public float padLeft { get; private set; } = -1;
        public float padRight { get; private set; } = -1;
        public float padTop { get; private set; } = -1;
        public float padBottom { get; private set; } = -1;

        internal static int[] GetSplits(Color[,] data)
        {
            int width = data.GetLength(0), height = data.GetLength(1);

            int startX = GetSplitPoint(data, 1, 0, true, true);
            int endX = GetSplitPoint(data, startX, 0, false, true);
            int startY = GetSplitPoint(data, 0, 1, true, false);
            int endY = GetSplitPoint(data, 0, startY, false, false);

            // Ensure pixels after the end are not invalid.
            GetSplitPoint(data, endX + 1, 0, true, true);
            GetSplitPoint(data, 0, endY + 1, true, false);

            // No splits, or all splits.
            if (startX == 0 && endX == 0 && startY == 0 && endY == 0) return null;

            // Subtraction here is because the coordinates were computed before the 1px border was stripped.
            if (startX != 0)
            {
                startX--;
                endX = width - 2 - (endX - 1);
            }
            else
            {
                // If no start point was ever found, we assume full stretch.
                endX = width - 2;
            }
            if (startY != 0)
            {
                startY--;
                endY = height - 2 - (endY - 1);
            }
            else
            {
                // If no start point was ever found, we assume full stretch.
                endY = height - 2;
            }

            return new int[] { startX, endX, startY, endY };
        }

        internal static int GetSplitPoint(Color[,] texture, int startX, int startY, bool startPoint, bool xAxis)
        {
            Color pixel;

            int next = xAxis ? startX : startY;
            int end = xAxis ? texture.GetLength(0) : texture.GetLength(1);
            int breakA = startPoint ? 255 : 0;

            int x = startX;
            int y = startY;
            while (next != end)
            {
                if (xAxis)
                    x = next;
                else
                    y = next;

                pixel = texture[x, y];
                if (pixel.A == breakA) return next;

                if (!startPoint && (pixel.R != 0 || pixel.G != 0 || pixel.B != 0 || pixel.A != 255)) throw new Exception("split error");//splitError(x, y, rgba, name);

                next++;
            }

            return 0;
        }

        internal static int[] GetPads(Color[,] data, int[] splits)
        {
            int width = data.GetLength(0), height = data.GetLength(1);
            int bottom = height - 1;
            int right = width - 1;

            int startX = GetSplitPoint(data, 1, bottom, true, true);
            int startY = GetSplitPoint(data, right, 1, true, false);

            // No need to hunt for the end if a start was never found.
            int endX = 0;
            int endY = 0;
            if (startX != 0) endX = GetSplitPoint(data, startX + 1, bottom, false, true);
            if (startY != 0) endY = GetSplitPoint(data, right, startY + 1, false, false);

            // Ensure pixels after the end are not invalid.
            GetSplitPoint(data, endX + 1, bottom, true, true);
            GetSplitPoint(data, right, endY + 1, true, false);

            // No pads.
            if (startX == 0 && endX == 0 && startY == 0 && endY == 0)
            {
                return null;
            }

            // -2 here is because the coordinates were computed before the 1px border was stripped.
            if (startX == 0 && endX == 0)
            {
                startX = -1;
                endX = -1;
            }
            else
            {
                if (startX > 0)
                {
                    startX--;
                    endX = width - 2 - (endX - 1);
                }
                else
                {
                    // If no start point was ever found, we assume full stretch.
                    endX = height - 2;
                }
            }
            if (startY == 0 && endY == 0)
            {
                startY = -1;
                endY = -1;
            }
            else
            {
                if (startY > 0)
                {
                    startY--;
                    endY = height - 2 - (endY - 1);
                }
                else
                {
                    // If no start point was ever found, we assume full stretch.
                    endY = width - 2;
                }
            }

            int[] pads = new int[] { startX, endX, startY, endY };

            if (splits != null && splits.Equals(pads))
            {
                return null;
            }

            return pads;
        }

        internal void SetPads(float left, float right, float top, float bottom)
        {
            padLeft = left;
            padRight = right;
            padTop = top;
            padBottom = bottom;
        }

        public NinePatch(TextureRegion region, int left, int right, int top, int bottom)
        {
            if (region == null)
            {
                throw new Exception("Region can't be null");
            }
            int middleWidth = region.GetWidth() - left - right;
            int middleHeight = region.GetHeight() - top - bottom;

            var patches = new TextureRegion[9];
            if (top > 0)
            {
                if (left > 0) patches[TOP_LEFT] = new TextureRegion(region, 0, 0, left, top);
                if (middleWidth > 0) patches[TOP_CENTER] = new TextureRegion(region, left, 0, middleWidth, top);
                if (right > 0) patches[TOP_RIGHT] = new TextureRegion(region, left + middleWidth, 0, right, top);
            }
            if (middleHeight > 0)
            {
                if (left > 0) patches[MIDDLE_LEFT] = new TextureRegion(region, 0, top, left, middleHeight);
                if (middleWidth > 0) patches[MIDDLE_CENTER] = new TextureRegion(region, left, top, middleWidth, middleHeight);
                if (right > 0) patches[MIDDLE_RIGHT] = new TextureRegion(region, left + middleWidth, top, right, middleHeight);
            }
            if (bottom > 0)
            {
                if (left > 0) patches[BOTTOM_LEFT] = new TextureRegion(region, 0, top + middleHeight, left, bottom);
                if (middleWidth > 0) patches[BOTTOM_CENTER] = new TextureRegion(region, left, top + middleHeight, middleWidth, bottom);
                if (right > 0) patches[BOTTOM_RIGHT] = new TextureRegion(region, left + middleWidth, top + middleHeight, right, bottom);
            }

            // If split only vertical, move splits from right to center.
            if (left == 0 && middleWidth == 0)
            {
                patches[TOP_CENTER] = patches[TOP_RIGHT];
                patches[MIDDLE_CENTER] = patches[MIDDLE_RIGHT];
                patches[BOTTOM_CENTER] = patches[BOTTOM_RIGHT];
                patches[TOP_RIGHT] = null;
                patches[MIDDLE_RIGHT] = null;
                patches[BOTTOM_RIGHT] = null;
            }
            // If split only horizontal, move splits from bottom to center.
            if (top == 0 && middleHeight == 0)
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
            _patches = new TextureRegion[9];
            if (patches[BOTTOM_LEFT] != null)
            {
                _patches[BOTTOM_LEFT] = patches[BOTTOM_LEFT];
                leftWidth = patches[BOTTOM_LEFT].GetWidth();
                bottomHeight = patches[BOTTOM_LEFT].GetHeight();
            }
            if (patches[BOTTOM_CENTER] != null)
            {
                _patches[BOTTOM_CENTER] = patches[BOTTOM_CENTER];
                middleWidth = Math.Max(middleWidth, patches[BOTTOM_CENTER].GetWidth());
                bottomHeight = Math.Max(bottomHeight, patches[BOTTOM_CENTER].GetHeight());
            }
            if (patches[BOTTOM_RIGHT] != null)
            {
                _patches[BOTTOM_RIGHT] = patches[BOTTOM_RIGHT];
                rightWidth = Math.Max(rightWidth, patches[BOTTOM_RIGHT].GetWidth());
                bottomHeight = Math.Max(bottomHeight, patches[BOTTOM_RIGHT].GetHeight());
            }
            if (patches[MIDDLE_LEFT] != null)
            {
                _patches[MIDDLE_LEFT] = patches[MIDDLE_LEFT];
                leftWidth = Math.Max(leftWidth, patches[MIDDLE_LEFT].GetWidth());
                middleHeight = Math.Max(middleHeight, patches[MIDDLE_LEFT].GetHeight());
            }
            if (patches[MIDDLE_CENTER] != null)
            {
                _patches[MIDDLE_CENTER] = patches[MIDDLE_CENTER];
                middleWidth = Math.Max(middleWidth, patches[MIDDLE_CENTER].GetWidth());
                middleHeight = Math.Max(middleHeight, patches[MIDDLE_CENTER].GetHeight());
            }
            if (patches[MIDDLE_RIGHT] != null)
            {
                _patches[MIDDLE_RIGHT] = patches[MIDDLE_RIGHT];
                rightWidth = Math.Max(rightWidth, patches[MIDDLE_RIGHT].GetWidth());
                middleHeight = Math.Max(middleHeight, patches[MIDDLE_RIGHT].GetHeight());
            }
            if (patches[TOP_LEFT] != null)
            {
                _patches[TOP_LEFT] = patches[TOP_LEFT];
                leftWidth = Math.Max(leftWidth, patches[TOP_LEFT].GetWidth());
                topHeight = Math.Max(topHeight, patches[TOP_LEFT].GetHeight());
            }
            if (patches[TOP_CENTER] != null)
            {
                _patches[TOP_CENTER] = patches[TOP_CENTER];
                middleWidth = Math.Max(middleWidth, patches[TOP_CENTER].GetWidth());
                topHeight = Math.Max(topHeight, patches[TOP_CENTER].GetHeight());
            }
            if (patches[TOP_RIGHT] != null)
            {
                _patches[TOP_RIGHT] = patches[TOP_RIGHT];
                rightWidth = Math.Max(rightWidth, patches[TOP_RIGHT].GetWidth());
                topHeight = Math.Max(topHeight, patches[TOP_RIGHT].GetHeight());
            }
        }

        public void Draw(Graphics graphics, float x, float y, float width, float height, Color? color = null)
        {
            if (!graphics.HasBegun)
            {
                graphics.Begin();
            }
            
            var col = color ?? Color.White;

            float centerColumnX = x + leftWidth;
            float rightColumnX = x + width - rightWidth;
            float middleRowY = y + topHeight;
            float topRowY = y + height - bottomHeight;

            if (_patches[TOP_LEFT] != null)
                _patches[TOP_LEFT].Draw(graphics, x, y, centerColumnX - x, middleRowY - y, col);
            if (_patches[TOP_CENTER] != null)
                _patches[TOP_CENTER].Draw(graphics, centerColumnX, y, rightColumnX - centerColumnX, middleRowY - y, col);
            if (_patches[TOP_RIGHT] != null)
                _patches[TOP_RIGHT].Draw(graphics, rightColumnX, y, x + width - rightColumnX, middleRowY - y, col);

            if (_patches[MIDDLE_LEFT] != null)
                _patches[MIDDLE_LEFT].Draw(graphics, x, middleRowY, centerColumnX - x, topRowY - middleRowY, col);
            if (_patches[MIDDLE_CENTER] != null)
                _patches[MIDDLE_CENTER].Draw(graphics, centerColumnX, middleRowY, rightColumnX - centerColumnX, topRowY - middleRowY, col);
            if (_patches[MIDDLE_RIGHT] != null)
                _patches[MIDDLE_RIGHT].Draw(graphics, rightColumnX, middleRowY, x + width - rightColumnX, topRowY - middleRowY, col);

            if (_patches[BOTTOM_LEFT] != null)
                _patches[BOTTOM_LEFT].Draw(graphics, x, topRowY, centerColumnX - x, y + height - topRowY, col);
            if (_patches[BOTTOM_CENTER] != null)
                _patches[BOTTOM_CENTER].Draw(graphics, centerColumnX, topRowY, rightColumnX - centerColumnX, y + height - topRowY, col);
            if (_patches[BOTTOM_RIGHT] != null)
                _patches[BOTTOM_RIGHT].Draw(graphics, rightColumnX, topRowY, x + width - rightColumnX, y + height - topRowY, col);

            /*if (_patches[TOP_CENTER] != null)
            {
                _patches[TOP_LEFT].Draw(graphics, x, y, _left, _top, col);
                _patches[TOP_CENTER].Draw(graphics, x + _left, y, width - _left - _right, _top, col);
                _patches[TOP_RIGHT].Draw(graphics, x + width - _right, y, _right, _top, col);
            }
               

            if (_patches[MIDDLE_CENTER] != null)
            {
                _patches[MIDDLE_LEFT].Draw(graphics, x, y + _top, _left, height - _top - _bottom, col);
                _patches[MIDDLE_CENTER].Draw(graphics, x + _left, y + _top, width - _left - _right, height - _top - _bottom, col);
                _patches[MIDDLE_RIGHT].Draw(graphics, x + width - _right, y + _top, _right, height - _top - _bottom, col);
            }

            if (_patches[BOTTOM_CENTER] != null)
            {
                _patches[BOTTOM_LEFT].Draw(graphics, x, y + height - _bottom, _left, _bottom, col);
                _patches[BOTTOM_CENTER].Draw(graphics, x + _left, y + height - _bottom, width - _left - _right, _bottom, col);
                _patches[BOTTOM_RIGHT].Draw(graphics, x + width - _right, y + height - _bottom, _right, _bottom, col);
            }*/

            //graphics.Flush();


        }

    }
}
