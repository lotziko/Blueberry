using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore.UI
{
    public class HorizontalGroup : Group
    {
        private float prefWidth, prefHeight, lastPrefHeight;
        private bool sizeInvalid = true;
        private List<float> rowSizes; // row width, row height, ...

        private int align = AlignInternal.left, rowAlign;
        private bool reverse, round = true, wrap, expand;
        private float space, wrapSpace, fill, padTop, padLeft, padBottom, padRight;

        public HorizontalGroup()
        {
            SetTouchable(Touchable.ChildrenOnly);
        }

        #region ILayout

        public override void Invalidate()
        {
            base.Invalidate();
            sizeInvalid = true;
        }

        private void ComputeSize()
        {
            sizeInvalid = false;
            List<Element> elements = GetElements();
            int n = elements.Count;
            prefHeight = 0;
            if (wrap)
            {
                prefWidth = 0;
                if (this.rowSizes == null)
                    this.rowSizes = new List<float>();
                else
                    this.rowSizes.Clear();
                var rowSizes = this.rowSizes;
                float space = this.space, wrapSpace = this.wrapSpace;
                float pad = padLeft + padRight, groupWidth = GetWidth() - pad, x = 0, y = 0, rowHeight = 0;
                int i = 0, incr = 1;
                if (reverse)
                {
                    i = n - 1;
                    n = -1;
                    incr = -1;
                }
                for (; i != n; i += incr)
                {
                    var element = elements[i];

                    float width, height;
                    if (element is ILayout)
                    {
                        var layout = (ILayout)element;
                        width = layout.PreferredWidth;
                        if (width > groupWidth) width = Math.Max(groupWidth, layout.MinWidth);
                        height = layout.PreferredHeight;
                    }
                    else
                    {
                        width = element.GetWidth();
                        height = element.GetHeight();
                    }

                    float incrX = width + (x > 0 ? space : 0);
                    if (x + incrX > groupWidth && x > 0)
                    {
                        rowSizes.Add(x);
                        rowSizes.Add(rowHeight);
                        prefWidth = Math.Max(prefWidth, x + pad);
                        if (y > 0) y += wrapSpace;
                        y += rowHeight;
                        rowHeight = 0;
                        x = 0;
                        incrX = width;
                    }
                    x += incrX;
                    rowHeight = Math.Max(rowHeight, height);
                }
                rowSizes.Add(x);
                rowSizes.Add(rowHeight);
                prefWidth = Math.Max(prefWidth, x + pad);
                if (y > 0) y += wrapSpace;
                prefHeight = Math.Max(prefHeight, y + rowHeight);
            }
            else
            {
                prefWidth = padLeft + padRight + space * (n - 1);
                for (int i = 0; i < n; i++)
                {
                    var element = elements[i];
                    if (element is ILayout)
                    {

                        var layout = (ILayout)element;
                        prefWidth += layout.PreferredWidth;
                        prefHeight = Math.Max(prefHeight, layout.PreferredHeight);
                    }
                    else
                    {
                        prefWidth += element.GetWidth();
                        prefHeight = Math.Max(prefHeight, element.GetHeight());
                    }
                }
            }
            prefHeight += padTop + padBottom;
            if (round)
            {
                prefWidth = MathF.Round(prefWidth);
                prefHeight = MathF.Round(prefHeight);
            }
        }

        public override void Layout()
        {
            if (sizeInvalid) ComputeSize();

            if (wrap)
            {
                LayoutWrapped();
                return;
            }

            bool round = this.round;
            int align = this.align;
            float space = this.space, padBottom = this.padBottom, fill = this.fill;
            float rowHeight = (expand ? GetHeight() : prefHeight) - padTop - padBottom, x = padLeft;

            if ((align & AlignInternal.right) != 0)
                x += GetWidth() - prefWidth;
            else if ((align & AlignInternal.left) == 0) // center
                x += (GetWidth() - prefWidth) / 2;

            float startY;
            if ((align & AlignInternal.top) != 0)
                startY = padTop;
            else if ((align & AlignInternal.bottom) != 0)
                startY = GetHeight() - padBottom - rowHeight;
            else
                startY = padTop + (GetHeight() - padTop - padBottom - rowHeight) / 2;

            align = rowAlign;

            var elements = GetElements();
            int i = 0, n = elements.Count, incr = 1;
            if (reverse)
            {
                i = n - 1;
                n = -1;
                incr = -1;
            }
            for (; i != n; i += incr)
            {
                var element = elements[i];

                float width, height;
                ILayout layout = null;
                if (element is ILayout)
                {
                    layout = (ILayout)element;
                    width = layout.PreferredWidth;
                    height = layout.PreferredHeight;
                }
                else
                {
                    width = element.GetWidth();
                    height = element.GetHeight();
                }

                if (fill > 0) height = rowHeight * fill;

                if (layout != null)
                {
                    height = Math.Max(height, layout.MinHeight);
                    float maxHeight = layout.MaxHeight;
                    if (maxHeight > 0 && height > maxHeight) height = maxHeight;
                }

                float y = startY;
                if ((align & AlignInternal.bottom) != 0)
                    y += rowHeight - height;
                else if ((align & AlignInternal.top) == 0) // center
                    y += (rowHeight - height) / 2;

                if (round)
                    element.SetBounds(MathF.Round(x), MathF.Round(y), MathF.Round(width), MathF.Round(height));
                else
                    element.SetBounds(x, y, width, height);
                x += width + space;

                if (layout != null) layout.Validate();
            }
        }

        private void LayoutWrapped()
        {
            float prefHeight = PreferredHeight;
            if (prefHeight != lastPrefHeight)
            {
                lastPrefHeight = prefHeight;
                InvalidateHierarchy();
            }

            int align = this.align;
            bool round = this.round;
            float space = this.space, padBottom = this.padBottom, fill = this.fill, wrapSpace = this.wrapSpace;
            float maxWidth = prefWidth - padLeft - padRight;
            float rowY = prefHeight - padTop, groupWidth = GetWidth(), xStart = padLeft, x = 0, rowHeight = 0;

            if ((align & AlignInternal.bottom) != 0)
                rowY += GetHeight() - prefHeight;
            else if ((align & AlignInternal.top) == 0) // center
                rowY += (GetHeight() - prefHeight) / 2;

            if ((align & AlignInternal.right) != 0)
                xStart += groupWidth - prefWidth;
            else if ((align & AlignInternal.left) == 0) // center
                xStart += (groupWidth - prefWidth) / 2;

            groupWidth -= padRight;
            align = this.rowAlign;

            var rowSizes = this.rowSizes;
            var elements = GetElements();
            int i = 0, n = elements.Count, incr = 1;
            if (reverse)
            {
                i = n - 1;
                n = -1;
                incr = -1;
            }
            for (int r = 0; i != n; i += incr)
            {
                var element = elements[i];

                float width, height;
                ILayout layout = null;
                if (element is ILayout) {
                layout = (ILayout)element;
                width = layout.PreferredWidth;
                if (width > groupWidth) width = Math.Max(groupWidth, layout.MinWidth);
                height = layout.PreferredHeight;
            } else {
                width = element.GetWidth();
                height = element.GetHeight();
            }

            if (x + width > groupWidth || r == 0)
            {
                x = xStart;
                if ((align & AlignInternal.right) != 0)
                    x += maxWidth - rowSizes[r];
                else if ((align & AlignInternal.left) == 0) // center
                    x += (maxWidth - rowSizes[r]) / 2;
                rowHeight = rowSizes[r + 1];
                if (r > 0) rowY -= wrapSpace;
                rowY -= rowHeight;
                r += 2;
            }

            if (fill > 0) height = rowHeight * fill;

            if (layout != null)
            {
                height = Math.Max(height, layout.MinHeight);
                float maxHeight = layout.MaxHeight;
                if (maxHeight > 0 && height > maxHeight) height = maxHeight;
            }

            float y = rowY;
            if ((align & AlignInternal.bottom) != 0)
                y += rowHeight - height;
            else if ((align & AlignInternal.top) == 0) // center
                y += (rowHeight - height) / 2;

            if (round)
                element.SetBounds(MathF.Round(x), MathF.Round(y), MathF.Round(width), MathF.Round(height));
            else
                element.SetBounds(x, y, width, height);
            x += width + space;

            if (layout != null) layout.Validate();
        }
    }

        public override float PreferredWidth
        {
            get
            {
                if (wrap) return 0;
                if (sizeInvalid) ComputeSize();
                return prefWidth;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                if (sizeInvalid) ComputeSize();
                return prefHeight;
            }
        }

        #endregion

        /** If true (the default), positions and sizes are rounded to integers. */
        public void SetRound(bool round)
        {
            this.round = round;
        }

        /** The children will be displayed last to first. */
        public HorizontalGroup Reverse()
        {
            this.reverse = true;
            return this;
        }

        /** If true, the children will be displayed last to first. */
        public HorizontalGroup Reverse(bool reverse)
        {
            this.reverse = reverse;
            return this;
        }

        public bool GetReverse()
        {
            return reverse;
        }

        /** Sets the vertical space between children. */
        public HorizontalGroup Space(float space)
        {
            this.space = space;
            return this;
        }

        public float GetSpace()
        {
            return space;
        }

        /** Sets the horizontal space between columns when wrap is enabled. */
        public HorizontalGroup WrapSpace(float wrapSpace)
        {
            this.wrapSpace = wrapSpace;
            return this;
        }

        public float GetWrapSpace()
        {
            return wrapSpace;
        }

        /** Sets the padTop, padLeft, padBottom, and padRight to the specified value. */
        public HorizontalGroup Pad(float pad)
        {
            padTop = pad;
            padLeft = pad;
            padBottom = pad;
            padRight = pad;
            return this;
        }

        public HorizontalGroup Pad(float top, float left, float bottom, float right)
        {
            padTop = top;
            padLeft = left;
            padBottom = bottom;
            padRight = right;
            return this;
        }

        public HorizontalGroup PadTop(float padTop)
        {
            this.padTop = padTop;
            return this;
        }

        public HorizontalGroup PadLeft(float padLeft)
        {
            this.padLeft = padLeft;
            return this;
        }

        public HorizontalGroup PadBottom(float padBottom)
        {
            this.padBottom = padBottom;
            return this;
        }

        public HorizontalGroup PadRight(float padRight)
        {
            this.padRight = padRight;
            return this;
        }

        public float GetPadTop()
        {
            return padTop;
        }

        public float GetPadLeft()
        {
            return padLeft;
        }

        public float GetPadBottom()
        {
            return padBottom;
        }

        public float GetPadRight()
        {
            return padRight;
        }

        /** Sets the alignment of all widgets within the horizontal group. Set to {@link Align#center}, {@link Align#top},
	 * {@link Align#bottom}, {@link Align#left}, {@link Align#right}, or any combination of those. */
        public HorizontalGroup Align(int align)
        {
            this.align = align;
            return this;
        }

        /** Sets the alignment of all widgets within the horizontal group to {@link Align#center}. This clears any other alignment. */
        public HorizontalGroup Center()
        {
            align = AlignInternal.center;
            return this;
        }

        /** Sets {@link Align#top} and clears {@link Align#bottom} for the alignment of all widgets within the horizontal group. */
        public HorizontalGroup Top()
        {
            align |= AlignInternal.top;
            align &= ~AlignInternal.bottom;
            return this;
        }

        /** Adds {@link Align#left} and clears {@link Align#right} for the alignment of all widgets within the horizontal group. */
        public HorizontalGroup Left()
        {
            align |= AlignInternal.left;
            align &= ~AlignInternal.right;
            return this;
        }

        /** Sets {@link Align#bottom} and clears {@link Align#top} for the alignment of all widgets within the horizontal group. */
        public HorizontalGroup Bottom()
        {
            align |= AlignInternal.bottom;
            align &= ~AlignInternal.top;
            return this;
        }

        /** Adds {@link Align#right} and clears {@link Align#left} for the alignment of all widgets within the horizontal group. */
        public HorizontalGroup Right()
        {
            align |= AlignInternal.right;
            align &= ~AlignInternal.left;
            return this;
        }

        public int GetAlign()
        {
            return align;
        }

        public HorizontalGroup Fill()
        {
            fill = 1f;
            return this;
        }

        /** @param fill 0 will use preferred width. */
        public HorizontalGroup Fill(float fill)
        {
            this.fill = fill;
            return this;
        }

        public float GetFill()
        {
            return fill;
        }

        public HorizontalGroup Expand()
        {
            expand = true;
            return this;
        }

        /** When true and wrap is false, the columns will take up the entire horizontal group height. */
        public HorizontalGroup Expand(bool expand)
        {
            this.expand = expand;
            return this;
        }

        public bool GetExpand()
        {
            return expand;
        }

        /** Sets fill to 1 and expand to true. */
        public HorizontalGroup Grow()
        {
            expand = true;
            fill = 1;
            return this;
        }

        /** If false, the widgets are arranged in a single row and the preferred width is the widget widths plus spacing.
	     * <p>
	     * If true, the widgets will wrap using the width of the horizontal group. The preferred width of the group will be 0 as it is
	     * expected that something external will set the width of the group. Widgets are sized to their preferred width unless it is
	     * larger than the group's width, in which case they are sized to the group's width but not less than their minimum width.
	     * Default is false.
	     * <p>
	     * When wrap is enabled, the group's preferred height depends on the width of the group. In some cases the parent of the group
	     * will need to layout twice: once to set the width of the group and a second time to adjust to the group's new preferred
	     * height. */
        public HorizontalGroup Wrap()
        {
            wrap = true;
            return this;
        }

        public HorizontalGroup Wrap(bool wrap)
        {
            this.wrap = wrap;
            return this;
        }

        public bool GetWrap()
        {
            return wrap;
        }

        /** Sets the horizontal alignment of each row of widgets when {@link #wrap() wrapping} is enabled and sets the vertical
         * alignment of widgets within each row. Set to {@link Align#center}, {@link Align#top}, {@link Align#bottom},
         * {@link Align#left}, {@link Align#right}, or any combination of those. */
        public HorizontalGroup RowAlign(int rowAlign)
        {
            this.rowAlign = rowAlign;
            return this;
        }

        /** Sets the alignment of widgets within each row to {@link Align#center}. This clears any other alignment. */
        public HorizontalGroup RowCenter()
        {
            rowAlign = AlignInternal.center;
            return this;
        }

        /** Sets {@link Align#top} and clears {@link Align#bottom} for the alignment of widgets within each row. */
        public HorizontalGroup RowTop()
        {
            rowAlign |= AlignInternal.top;
            rowAlign &= ~AlignInternal.bottom;
            return this;
        }

        /** Adds {@link Align#left} and clears {@link Align#right} for the alignment of each row of widgets when {@link #wrap()
         * wrapping} is enabled. */
        public HorizontalGroup RowLeft()
        {
            rowAlign |= AlignInternal.left;
            rowAlign &= ~AlignInternal.right;
            return this;
        }

        /** Sets {@link Align#bottom} and clears {@link Align#top} for the alignment of widgets within each row. */
        public HorizontalGroup RowBottom()
        {
            rowAlign |= AlignInternal.bottom;
            rowAlign &= ~AlignInternal.top;
            return this;
        }

        /** Adds {@link Align#right} and clears {@link Align#left} for the alignment of each row of widgets when {@link #wrap()
         * wrapping} is enabled. */
        public HorizontalGroup RowRight()
        {
            rowAlign |= AlignInternal.right;
            rowAlign &= ~AlignInternal.left;
            return this;
        }

        public override void DrawDebug(Graphics graphics)
        {
            base.DrawDebug(graphics);
            if (!GetDebug()) return;
            graphics.DrawRectangleBorder(GetX() + padLeft, GetY() + padBottom, GetWidth() - padLeft - padRight, GetHeight() - padBottom - padTop, Table.debugElementColor);
        }
    }
}
