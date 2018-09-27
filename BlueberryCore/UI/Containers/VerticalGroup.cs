using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore.UI
{
    public class VerticalGroup : Group
    {
        private float prefWidth, prefHeight, lastPrefWidth;
        private bool sizeInvalid = true;
        private List<float> columnSizes; // column height, column width, ...

        private int align = AlignInternal.top, columnAlign;
        private bool reverse, round = true, wrap, expand;
        private float space, wrapSpace, fill, padTop, padLeft, padBottom, padRight;

        public VerticalGroup()
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
            var elements = GetElements();
            int n = elements.Count;
            prefWidth = 0;
            if (wrap)
            {
                prefHeight = 0;
                if (this.columnSizes == null)
                    this.columnSizes = new List<float>();
                else
                    this.columnSizes.Clear();
                var columnSizes = this.columnSizes;
                float space = this.space, wrapSpace = this.wrapSpace;
                float pad = padTop + padBottom, groupHeight = GetHeight() - pad, x = 0, y = 0, columnWidth = 0;
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
                        height = layout.PreferredHeight;
                        if (height > groupHeight) height = Math.Max(groupHeight, layout.MinHeight);
                    }
                    else
                    {
                        width = element.GetWidth();
                        height = element.GetHeight();
                    }

                    float incrY = height + (y > 0 ? space : 0);
                    if (y + incrY > groupHeight && y > 0)
                    {
                        columnSizes.Add(y);
                        columnSizes.Add(columnWidth);
                        prefHeight = Math.Max(prefHeight, y + pad);
                        if (x > 0) x += wrapSpace;
                        x += columnWidth;
                        columnWidth = 0;
                        y = 0;
                        incrY = height;
                    }
                    y += incrY;
                    columnWidth = Math.Max(columnWidth, width);
                }
                columnSizes.Add(y);
                columnSizes.Add(columnWidth);
                prefHeight = Math.Max(prefHeight, y + pad);
                if (x > 0) x += wrapSpace;
                prefWidth = Math.Max(prefWidth, x + columnWidth);
            }
            else
            {
                prefHeight = padTop + padBottom + space * (n - 1);
                for (int i = 0; i < n; i++)
                {
                    var element = elements[i];
                    if (element is ILayout)
                    {

                        var layout = (ILayout)element;
                        prefWidth = Math.Max(prefWidth, layout.PreferredWidth);
                        prefHeight += layout.PreferredHeight;
                    }
                    else
                    {
                        prefWidth = Math.Max(prefWidth, element.GetWidth());
                        prefHeight += element.GetHeight();
                    }
                }
            }
            prefWidth += padLeft + padRight;
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
            float space = this.space, padLeft = this.padLeft, fill = this.fill;
            float columnWidth = (expand ? GetWidth() : prefWidth) - padLeft - padRight, y = /*prefHeight - */padTop + space;

            if ((align & AlignInternal.bottom) != 0)
                y += GetHeight() - prefHeight;
            else if ((align & AlignInternal.top) == 0) // center
                y += (GetHeight() - prefHeight) / 2;

            float startX;
            if ((align & AlignInternal.left) != 0)
                startX = padLeft;
            else if ((align & AlignInternal.right) != 0)
                startX = GetWidth() - padRight - columnWidth;
            else
                startX = padLeft + (GetWidth() - padLeft - padRight - columnWidth) / 2;

            align = columnAlign;

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

                if (fill > 0) width = columnWidth * fill;

                if (layout != null)
                {
                    width = Math.Max(width, layout.MinWidth);
                    float maxWidth = layout.MaxWidth;
                    if (maxWidth > 0 && width > maxWidth) width = maxWidth;
                }

                float x = startX;
                if ((align & AlignInternal.right) != 0)
                    x += columnWidth - width;
                else if ((align & AlignInternal.left) == 0) // center
                    x += (columnWidth - width) / 2;

                //y /*-*/+= height + space;
                if (round)
                    element.SetBounds(MathF.Round(x), MathF.Round(y), MathF.Round(width), MathF.Round(height));
                else
                    element.SetBounds(x, y, width, height);
                y /*-*/+= height + space;

                if (layout != null) layout.Validate();
            }
        }

        private void LayoutWrapped()
        {
            float prefWidth = PreferredWidth;
            if (prefWidth != lastPrefWidth)
            {
                lastPrefWidth = prefWidth;
                InvalidateHierarchy();
            }

            int align = this.align;
            bool round = this.round;
            float space = this.space, padLeft = this.padLeft, fill = this.fill, wrapSpace = this.wrapSpace;
            float maxHeight = prefHeight - padTop - padBottom;
            float columnX = padLeft, groupHeight = GetHeight();
            float yStart = prefHeight - padTop + space, y = 0, columnWidth = 0;

            if ((align & AlignInternal.right) != 0)
                columnX += GetWidth() - prefWidth;
            else if ((align & AlignInternal.left) == 0) // center
                columnX += (GetWidth() - prefWidth) / 2;

            if ((align & AlignInternal.bottom) != 0)
                yStart += groupHeight - prefHeight;
            else if ((align & AlignInternal.top) == 0) // center
                yStart += (groupHeight - prefHeight) / 2;

            groupHeight -= padTop;
            align = columnAlign;

            var columnSizes = this.columnSizes;
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
                height = layout.PreferredHeight;
                if (height > groupHeight) height = Math.Max(groupHeight, layout.MinHeight);
            } else {
                width = element.GetWidth();
                height = element.GetHeight();
            }

            if (y - height - space < padBottom || r == 0)
            {
                y = yStart;
                if ((align & AlignInternal.top) != 0)
                    y -= maxHeight - columnSizes[r];
                else if ((align & AlignInternal.bottom) == 0) // center
                    y -= (maxHeight - columnSizes[r]) / 2;
                if (r > 0)
                {
                    columnX += wrapSpace;
                    columnX += columnWidth;
                }
                columnWidth = columnSizes[r + 1];
                r += 2;
            }

            if (fill > 0) width = columnWidth * fill;

            if (layout != null)
            {
                width = Math.Max(width, layout.MinWidth);
                float maxWidth = layout.MaxWidth;
                if (maxWidth > 0 && width > maxWidth) width = maxWidth;
            }

            float x = columnX;
            if ((align & AlignInternal.right) != 0)
                x += columnWidth - width;
            else if ((align & AlignInternal.left) == 0) // center
                x += (columnWidth - width) / 2;

            y -= height + space;
            if (round)
                element.SetBounds(MathF.Round(x), MathF.Round(y), MathF.Round(width), MathF.Round(height));
            else
                element.SetBounds(x, y, width, height);

                if (layout != null) layout.Validate();
            }
        }

        public override float PreferredWidth
        {
            get
            {
                if (sizeInvalid) ComputeSize();
                return prefWidth;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                if (wrap) return 0;
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
        public VerticalGroup Reverse()
        {
            this.reverse = true;
            return this;
        }

        /** If true, the children will be displayed last to first. */
        public VerticalGroup Reverse(bool reverse)
        {
            this.reverse = reverse;
            return this;
        }

        public bool GetReverse()
        {
            return reverse;
        }

        /** Sets the vertical space between children. */
        public VerticalGroup Space(float space)
        {
            this.space = space;
            return this;
        }

        public float GetSpace()
        {
            return space;
        }

        /** Sets the horizontal space between columns when wrap is enabled. */
        public VerticalGroup WrapSpace(float wrapSpace)
        {
            this.wrapSpace = wrapSpace;
            return this;
        }

        public float GetWrapSpace()
        {
            return wrapSpace;
        }

        /** Sets the padTop, padLeft, padBottom, and padRight to the specified value. */
        public VerticalGroup Pad(float pad)
        {
            padTop = pad;
            padLeft = pad;
            padBottom = pad;
            padRight = pad;
            return this;
        }

        public VerticalGroup Pad(float top, float left, float bottom, float right)
        {
            padTop = top;
            padLeft = left;
            padBottom = bottom;
            padRight = right;
            return this;
        }

        public VerticalGroup PadTop(float padTop)
        {
            this.padTop = padTop;
            return this;
        }

        public VerticalGroup PadLeft(float padLeft)
        {
            this.padLeft = padLeft;
            return this;
        }

        public VerticalGroup PadBottom(float padBottom)
        {
            this.padBottom = padBottom;
            return this;
        }

        public VerticalGroup PadRight(float padRight)
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

        /** Sets the alignment of all widgets within the vertical group. Set to {@link Align#center}, {@link Align#top},
	 * {@link Align#bottom}, {@link Align#left}, {@link Align#right}, or any combination of those. */
        public VerticalGroup Align(int align)
        {
            this.align = align;
            return this;
        }

        /** Sets the alignment of all widgets within the vertical group to {@link Align#center}. This clears any other alignment. */
        public VerticalGroup Center()
        {
            align = AlignInternal.center;
            return this;
        }

        /** Sets {@link Align#top} and clears {@link Align#bottom} for the alignment of all widgets within the vertical group. */
        public VerticalGroup Top()
        {
            align |= AlignInternal.top;
            align &= ~AlignInternal.bottom;
            return this;
        }

        /** Adds {@link Align#left} and clears {@link Align#right} for the alignment of all widgets within the vertical group. */
        public VerticalGroup Left()
        {
            align |= AlignInternal.left;
            align &= ~AlignInternal.right;
            return this;
        }

        /** Sets {@link Align#bottom} and clears {@link Align#top} for the alignment of all widgets within the vertical group. */
        public VerticalGroup Bottom()
        {
            align |= AlignInternal.bottom;
            align &= ~AlignInternal.top;
            return this;
        }

        /** Adds {@link Align#right} and clears {@link Align#left} for the alignment of all widgets within the vertical group. */
        public VerticalGroup Right()
        {
            align |= AlignInternal.right;
            align &= ~AlignInternal.left;
            return this;
        }

        public int GetAlign()
        {
            return align;
        }

        public VerticalGroup Fill()
        {
            fill = 1f;
            return this;
        }

        /** @param fill 0 will use preferred height. */
        public VerticalGroup Fill(float fill)
        {
            this.fill = fill;
            return this;
        }

        public float GetFill()
        {
            return fill;
        }

        public VerticalGroup Expand()
        {
            expand = true;
            return this;
        }

        /** When true and wrap is false, the columns will take up the entire vertical group width. */
        public VerticalGroup Expand(bool expand)
        {
            this.expand = expand;
            return this;
        }

        public bool GetExpand()
        {
            return expand;
        }

        /** Sets fill to 1 and expand to true. */
        public VerticalGroup Grow()
        {
            expand = true;
            fill = 1;
            return this;
        }

        /** If false, the widgets are arranged in a single column and the preferred height is the widget heights plus spacing.
	     * <p>
	     * If true, the widgets will wrap using the height of the vertical group. The preferred height of the group will be 0 as it is
	     * expected that something external will set the height of the group. Widgets are sized to their preferred height unless it is
	     * larger than the group's height, in which case they are sized to the group's height but not less than their minimum height.
	     * Default is false.
	     * <p>
	     * When wrap is enabled, the group's preferred width depends on the height of the group. In some cases the parent of the group
	     * will need to layout twice: once to set the height of the group and a second time to adjust to the group's new preferred
	     * width. */
        public VerticalGroup Wrap()
        {
            wrap = true;
            return this;
        }

        public VerticalGroup Wrap(bool wrap)
        {
            this.wrap = wrap;
            return this;
        }

        public bool GetWrap()
        {
            return wrap;
        }

        /** Sets the vertical alignment of each column of widgets when {@link #wrap() wrapping} is enabled and sets the horizontal
         * alignment of widgets within each column. Set to {@link Align#center}, {@link Align#top}, {@link Align#bottom},
         * {@link Align#left}, {@link Align#right}, or any combination of those. */
        public VerticalGroup ColumnAlign(int columnAlign)
        {
            this.columnAlign = columnAlign;
            return this;
        }

        /** Sets the alignment of widgets within each column to {@link Align#center}. This clears any other alignment. */
        public VerticalGroup ColumnCenter()
        {
            columnAlign = AlignInternal.center;
            return this;
        }

        /** Adds {@link Align#top} and clears {@link Align#bottom} for the alignment of each column of widgets when {@link #wrap()
         * wrapping} is enabled. */
        public VerticalGroup ColumnTop()
        {
            columnAlign |= AlignInternal.top;
            columnAlign &= ~AlignInternal.bottom;
            return this;
        }

        /** Adds {@link Align#left} and clears {@link Align#right} for the alignment of widgets within each column. */
        public VerticalGroup ColumnLeft()
        {
            columnAlign |= AlignInternal.left;
            columnAlign &= ~AlignInternal.right;
            return this;
        }

        /** Adds {@link Align#bottom} and clears {@link Align#top} for the alignment of each column of widgets when {@link #wrap()
         * wrapping} is enabled. */
        public VerticalGroup ColumnBottom()
        {
            columnAlign |= AlignInternal.bottom;
            columnAlign &= ~AlignInternal.top;
            return this;
        }

        /** Adds {@link Align#right} and clears {@link Align#left} for the alignment of widgets within each column. */
        public VerticalGroup ColumnRight()
        {
            columnAlign |= AlignInternal.right;
            columnAlign &= ~AlignInternal.left;
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
