using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blueberry.UI
{
    public class Cell : IPoolable
    {
        #region Fields

        static private bool files;
        static private Cell defaults;

        internal Value minWidth, minHeight;
        internal Value prefWidth, prefHeight;
        internal Value maxWidth, maxHeight;
        internal Value spaceTop, spaceLeft, spaceBottom, spaceRight;
        internal Value padTop, padLeft, padBottom, padRight;
        internal float? fillX, fillY;
        internal int? align;
        internal bool? ignore;
        internal int? expandX, expandY;
        internal int? colspan;
        internal bool? uniformX, uniformY;

        internal Element element;
        internal float elementX, elementY;
        internal float elementWidth, elementHeight;

        private Table table;
        internal bool endRow;
        internal int column, row;
        internal int cellAboveIndex;
        internal float computedPadTop, computedPadLeft, computedPadBottom, computedPadRight;

        #endregion

        public Cell()
        {
            Reset();
        }

        public void SetLayout(Table layout)
        {
            table = layout;
        }

        public Table GetLayout()
        {
            return table;
        }

        /** Sets the element in this cell and adds the element to the cell's table. If null, removes any current element. */
        public Cell SetElement(Element newElement)
        {
            if (element != newElement)
            {
                if (element != null)
                    element.Remove();
                element = newElement;
                if (newElement != null)
                    table.AddElement(newElement);
            }
            return this;
        }

        /** Returns the element for this cell, or null. */
        public Element GetElement()
        {
            return element;
        }

        /** Returns true if the cell's element is not null. */
        public bool HasElement()
        {
            return element != null;
        }

        #region Resizers/Setters

        /** Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified value. */
        public Cell Size(Value size)
        {
            minWidth = size;
            minHeight = size;
            prefWidth = size;
            prefHeight = size;
            maxWidth = size;
            maxHeight = size;
            return this;
        }

        /** Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified values. */
        public Cell Size(Value width, Value height)
        {
            minWidth = width;
            minHeight = height;
            prefWidth = width;
            prefHeight = height;
            maxWidth = width;
            maxHeight = height;
            return this;
        }

        /** Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified value. */
        public Cell Size(float size)
        {
            Size(new Value.Fixed(size));
            return this;
        }

        /** Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified values. */
        public Cell Size(float width, float height)
        {
            Size(new Value.Fixed(width), new Value.Fixed(height));
            return this;
        }

        /** Sets the minWidth, prefWidth, and maxWidth to the specified value. */
        public Cell Width(Value width)
        {
            minWidth = width;
            prefWidth = width;
            maxWidth = width;
            return this;
        }

        /** Sets the minWidth, prefWidth, and maxWidth to the specified value. */
        public Cell Width(float width)
        {
            Width(new Value.Fixed(width));
            return this;
        }

        /** Sets the minHeight, prefHeight, and maxHeight to the specified value. */
        public Cell Height(Value height)
        {
            minHeight = height;
            prefHeight = height;
            maxHeight = height;
            return this;
        }

        /** Sets the minHeight, prefHeight, and maxHeight to the specified value. */
        public Cell Height(float height)
        {
            Height(new Value.Fixed(height));
            return this;
        }

        /** Sets the minWidth and minHeight to the specified value. */
        public Cell MinSize(Value size)
        {
            minWidth = size;
            minHeight = size;
            return this;
        }

        /** Sets the minWidth and minHeight to the specified values. */
        public Cell MinSize(Value width, Value height)
        {
            minWidth = width;
            minHeight = height;
            return this;
        }

        public Cell MinWidth(Value minWidth)
        {
            this.minWidth = minWidth;
            return this;
        }

        public Cell MinHeight(Value minHeight)
        {
            this.minHeight = minHeight;
            return this;
        }

        /** Sets the minWidth and minHeight to the specified value. */
        public Cell MinSize(float size)
        {
            minWidth = new Value.Fixed(size);
            minHeight = new Value.Fixed(size);
            return this;
        }

        /** Sets the minWidth and minHeight to the specified values. */
        public Cell MinSize(float width, float height)
        {
            minWidth = new Value.Fixed(width);
            minHeight = new Value.Fixed(height);
            return this;
        }

        public Cell MinWidth(float minWidth)
        {
            this.minWidth = new Value.Fixed(minWidth);
            return this;
        }

        public Cell MinHeight(float minHeight)
        {
            this.minHeight = new Value.Fixed(minHeight);
            return this;
        }

        /** Sets the prefWidth and prefHeight to the specified value. */
        public Cell PrefSize(Value size)
        {
            prefWidth = size;
            prefHeight = size;
            return this;
        }

        /** Sets the prefWidth and prefHeight to the specified values. */
        public Cell PrefSize(Value width, Value height)
        {
            prefWidth = width;
            prefHeight = height;
            return this;
        }

        public Cell PrefWidth(Value prefWidth)
        {
            this.prefWidth = prefWidth;
            return this;
        }

        public Cell PrefHeight(Value prefHeight)
        {
            this.prefHeight = prefHeight;
            return this;
        }

        /** Sets the prefWidth and prefHeight to the specified value. */
        public Cell PrefSize(float width, float height)
        {
            prefWidth = new Value.Fixed(width);
            prefHeight = new Value.Fixed(height);
            return this;
        }

        /** Sets the prefWidth and prefHeight to the specified values. */
        public Cell PrefSize(float size)
        {
            prefWidth = new Value.Fixed(size);
            prefHeight = new Value.Fixed(size);
            return this;
        }

        public Cell PrefWidth(float prefWidth)
        {
            this.prefWidth = new Value.Fixed(prefWidth);
            return this;
        }

        public Cell PrefHeight(float prefHeight)
        {
            this.prefHeight = new Value.Fixed(prefHeight);
            return this;
        }

        /** Sets the maxWidth and maxHeight to the specified value. */
        public Cell MaxSize(Value size)
        {
            maxWidth = size;
            maxHeight = size;
            return this;
        }

        /** Sets the maxWidth and maxHeight to the specified values. */
        public Cell MaxSize(Value width, Value height)
        {
            maxWidth = width;
            maxHeight = height;
            return this;
        }

        public Cell MaxWidth(Value maxWidth)
        {
            this.maxWidth = maxWidth;
            return this;
        }

        public Cell MaxHeight(Value maxHeight)
        {
            this.maxHeight = maxHeight;
            return this;
        }

        /** Sets the maxWidth and maxHeight to the specified value. */
        public Cell MaxSize(float size)
        {
            maxWidth = new Value.Fixed(size);
            maxHeight = new Value.Fixed(size);
            return this;
        }

        /** Sets the maxWidth and maxHeight to the specified values. */
        public Cell MaxSize(float width, float height)
        {
            maxWidth = new Value.Fixed(width);
            maxHeight = new Value.Fixed(height);
            return this;
        }

        public Cell MaxWidth(float maxWidth)
        {
            this.maxWidth = new Value.Fixed(maxWidth);
            return this;
        }

        public Cell MaxHeight(float maxHeight)
        {
            this.maxHeight = new Value.Fixed(maxHeight);
            return this;
        }

        /** Sets the spaceTop, spaceLeft, spaceBottom, and spaceRight to the specified value. */
        public Cell Space(Value space)
        {
            spaceTop = space;
            spaceLeft = space;
            spaceBottom = space;
            spaceRight = space;
            return this;
        }

        public Cell Space(Value top, Value left, Value bottom, Value right)
        {
            spaceTop = top;
            spaceLeft = left;
            spaceBottom = bottom;
            spaceRight = right;
            return this;
        }

        public Cell SpaceTop(Value spaceTop)
        {
            this.spaceTop = spaceTop;
            return this;
        }

        public Cell SpaceLeft(Value spaceLeft)
        {
            this.spaceLeft = spaceLeft;
            return this;
        }

        public Cell SpaceBottom(Value spaceBottom)
        {
            this.spaceBottom = spaceBottom;
            return this;
        }

        public Cell SpaceRight(Value spaceRight)
        {
            this.spaceRight = spaceRight;
            return this;
        }

        /** Sets the spaceTop, spaceLeft, spaceBottom, and spaceRight to the specified value. */
        public Cell Space(float space)
        {
            if (space < 0) throw new ArgumentException("space cannot be < 0.");
            Value value = new Value.Fixed(space);
            spaceTop = value;
            spaceLeft = value;
            spaceBottom = value;
            spaceRight = value;
            return this;
        }

        public Cell Space(float top, float left, float bottom, float right)
        {
            if (top < 0) throw new ArgumentException("top cannot be < 0.");
            if (left < 0) throw new ArgumentException("left cannot be < 0.");
            if (bottom < 0) throw new ArgumentException("bottom cannot be < 0.");
            if (right < 0) throw new ArgumentException("right cannot be < 0.");
            spaceTop = new Value.Fixed(top);
            spaceLeft = new Value.Fixed(left);
            spaceBottom = new Value.Fixed(bottom);
            spaceRight = new Value.Fixed(right);
            return this;
        }

        public Cell SpaceTop(float spaceTop)
        {
            if (spaceTop < 0) throw new ArgumentException("spaceTop cannot be < 0.");
            this.spaceTop = new Value.Fixed(spaceTop);
            return this;
        }

        public Cell SpaceLeft(float spaceLeft)
        {
            if (spaceLeft < 0) throw new ArgumentException("spaceLeft cannot be < 0.");
            this.spaceLeft = new Value.Fixed(spaceLeft);
            return this;
        }

        public Cell SpaceBottom(float spaceBottom)
        {
            if (spaceBottom < 0) throw new ArgumentException("spaceBottom cannot be < 0.");
            this.spaceBottom = new Value.Fixed(spaceBottom);
            return this;
        }

        public Cell SpaceRight(float spaceRight)
        {
            if (spaceRight < 0) throw new ArgumentException("spaceRight cannot be < 0.");
            this.spaceRight = new Value.Fixed(spaceRight);
            return this;
        }

        /** Sets the padTop, padLeft, padBottom, and padRight to the specified value. */
        public Cell Pad(Value pad)
        {
            padTop = pad;
            padLeft = pad;
            padBottom = pad;
            padRight = pad;
            return this;
        }

        public Cell Pad(Value top, Value left, Value bottom, Value right)
        {
            padTop = top;
            padLeft = left;
            padBottom = bottom;
            padRight = right;
            return this;
        }

        public Cell PadTop(Value padTop)
        {
            this.padTop = padTop;
            return this;
        }

        public Cell PadLeft(Value padLeft)
        {
            this.padLeft = padLeft;
            return this;
        }

        public Cell PadBottom(Value padBottom)
        {
            this.padBottom = padBottom;
            return this;
        }

        public Cell PadRight(Value padRight)
        {
            this.padRight = padRight;
            return this;
        }

        /** Sets the padTop, padLeft, padBottom, and padRight to the specified value. */
        public Cell Pad(float pad)
        {
            Value value = new Value.Fixed(pad);
            padTop = value;
            padLeft = value;
            padBottom = value;
            padRight = value;
            return this;
        }

        public Cell Pad(float top, float left, float bottom, float right)
        {
            padTop = new Value.Fixed(top);
            padLeft = new Value.Fixed(left);
            padBottom = new Value.Fixed(bottom);
            padRight = new Value.Fixed(right);
            return this;
        }

        public Cell PadTop(float padTop)
        {
            this.padTop = new Value.Fixed(padTop);
            return this;
        }

        public Cell PadLeft(float padLeft)
        {
            this.padLeft = new Value.Fixed(padLeft);
            return this;
        }

        public Cell PadBottom(float padBottom)
        {
            this.padBottom = new Value.Fixed(padBottom);
            return this;
        }

        public Cell PadRight(float padRight)
        {
            this.padRight = new Value.Fixed(padRight);
            return this;
        }

        /** Sets fillX and fillY to 1. */
        public Cell Fill()
        {
            fillX = 1f;
            fillY = 1f;
            return this;
        }

        /** Sets fillX to 1. */
        public Cell FillX()
        {
            fillX = 1f;
            return this;
        }

        /** Sets fillY to 1. */
        public Cell FillY()
        {
            fillY = 1f;
            return this;
        }

        public Cell Fill(float x, float y)
        {
            fillX = x;
            fillY = y;
            return this;
        }

        /** Sets fillX and fillY to 1 if true, 0 if false. */
        public Cell Fill(bool x, bool y)
        {
            fillX = x ? 1f : 0;
            fillY = y ? 1f : 0;
            return this;
        }

        /** Sets fillX and fillY to 1 if true, 0 if false. */
        public Cell Fill(bool fill)
        {
            fillX = fill ? 1f : 0;
            fillY = fill ? 1f : 0;
            return this;
        }

        /** Sets the alignment of the element within the cell. Set to {@link #CENTER}, {@link #TOP}, {@link #BOTTOM}, {@link #LEFT},
	 * {@link #RIGHT}, or any combination of those. */
        public Cell Align(int align)
        {
            this.align = align;
            return this;
        }

        /** Sets the alignment of the element within the cell to {@link #CENTER}. This clears any other alignment. */
        public Cell Center()
        {
            align = AlignInternal.center;
            return this;
        }

        /** Adds {@link #TOP} and clears {@link #BOTTOM} for the alignment of the element within the cell. */
        public Cell Top()
        {
            if (align == null)
                align = AlignInternal.top;
            else
            {
                align |= AlignInternal.top;
                align &= ~AlignInternal.bottom;
            }
            return this;
        }

        /** Adds {@link #LEFT} and clears {@link #RIGHT} for the alignment of the element within the cell. */
        public Cell Left()
        {
            if (align == null)
                align = AlignInternal.left;
            else
            {
                align |= AlignInternal.left;
                align &= ~AlignInternal.right;
            }
            return this;
        }

        /** Adds {@link #BOTTOM} and clears {@link #TOP} for the alignment of the element within the cell. */
        public Cell Bottom()
        {
            if (align == null)
                align = AlignInternal.bottom;
            else
            {
                align |= AlignInternal.bottom;
                align &= ~AlignInternal.top;
            }
            return this;
        }

        /** Adds {@link #RIGHT} and clears {@link #LEFT} for the alignment of the element within the cell. */
        public Cell Right()
        {
            if (align == null)
                align = AlignInternal.right;
            else
            {
                align |= AlignInternal.right;
                align &= ~AlignInternal.left;
            }
            return this;
        }

        /** Sets expandX and expandY to 1. */
        public Cell Expand()
        {
            expandX = 1;
            expandY = 1;
            return this;
        }

        /** Sets expandX to 1. */
        public Cell ExpandX()
        {
            expandX = 1;
            return this;
        }

        /** Sets expandY to 1. */
        public Cell ExpandY()
        {
            expandY = 1;
            return this;
        }

        public Cell Expand(int x, int y)
        {
            expandX = x;
            expandY = y;
            return this;
        }

        /** Sets expandX and expandY to 1 if true, 0 if false. */
        public Cell Expand(bool x, bool y)
        {
            expandX = x ? 1 : 0;
            expandY = y ? 1 : 0;
            return this;
        }

        public Cell Ignore(Boolean ignore)
        {
            this.ignore = ignore;
            return this;
        }

        /** Sets ignore to true. */
        public Cell Ignore()
        {
            ignore = true;
            return this;
        }

        public bool GetIgnore()
        {
            return ignore != null && ignore == true;
        }

        public Cell Colspan(int colspan)
        {
            this.colspan = colspan;
            return this;
        }

        public void SetElementBounds(float x, float y, float width, float height)
        {
            elementX = x;
            elementY = y;
            elementWidth = width;
            elementHeight = height;
        }

        /** Sets uniformX and uniformY to true. */
        public Cell Uniform()
        {
            uniformX = true;
            uniformY = true;
            return this;
        }

        /** Sets uniformX to true. */
        public Cell UniformX()
        {
            uniformX = true;
            return this;
        }

        /** Sets uniformY to true. */
        public Cell UniformY()
        {
            uniformY = true;
            return this;
        }

        public Cell Uniform(Boolean x, Boolean y)
        {
            uniformX = x;
            uniformY = y;
            return this;
        }

        public float GetElementX()
        {
            return elementX;
        }

        public void SetElementX(float elementX)
        {
            this.elementX = elementX;
        }

        public float GetElementY()
        {
            return elementY;
        }

        public void SetElementY(float elementY)
        {
            this.elementY = elementY;
        }

        public float GetElementWidth()
        {
            return elementWidth;
        }

        public void SetElementWidth(float elementWidth)
        {
            this.elementWidth = elementWidth;
        }

        public float GetElementHeight()
        {
            return elementHeight;
        }

        public void SetElementHeight(float elementHeight)
        {
            this.elementHeight = elementHeight;
        }

        public int GetColumn()
        {
            return column;
        }

        public int GetRow()
        {
            return row;
        }

        /** @return May be null if this cell is row defaults. */
        public Value GetMinWidthValue()
        {
            return minWidth;
        }

        public float GetMinWidth()
        {
            return minWidth == null ? 0 : minWidth.Get(element);
        }

        /** @return May be null if this cell is row defaults. */
        public Value GetMinHeightValue()
        {
            return minHeight;
        }

        public float GetMinHeight()
        {
            return minHeight == null ? 0 : minHeight.Get(element);
        }

        /** @return May be null if this cell is row defaults. */
        public Value GetPrefWidthValue()
        {
            return prefWidth;
        }

        public float GetPrefWidth()
        {
            return prefWidth == null ? 0 : prefWidth.Get(element);
        }

        /** @return May be null if this cell is row defaults. */
        public Value GetPrefHeightValue()
        {
            return prefHeight;
        }

        public float GetPrefHeight()
        {
            return prefHeight == null ? 0 : prefHeight.Get(element);
        }

        /** @return May be null if this cell is row defaults. */
        public Value GetMaxWidthValue()
        {
            return maxWidth;
        }

        public float GetMaxWidth()
        {
            return maxWidth == null ? 0 : maxWidth.Get(element);
        }

        /** @return May be null if this cell is row defaults. */
        public Value GetMaxHeightValue()
        {
            return maxHeight;
        }

        public float GetMaxHeight()
        {
            return maxHeight == null ? 0 : maxHeight.Get(element);
        }

        /** @return May be null if this value is not set. */
        public Value GetSpaceTopValue()
        {
            return spaceTop;
        }

        public float GetSpaceTop()
        {
            return spaceTop == null ? 0 : spaceTop.Get(element);
        }

        /** @return May be null if this value is not set. */
        public Value GetSpaceLeftValue()
        {
            return spaceLeft;
        }

        public float GetSpaceLeft()
        {
            return spaceLeft == null ? 0 : spaceLeft.Get(element);
        }

        /** @return May be null if this value is not set. */
        public Value GetSpaceBottomValue()
        {
            return spaceBottom;
        }

        public float GetSpaceBottom()
        {
            return spaceBottom == null ? 0 : spaceBottom.Get(element);
        }

        /** @return May be null if this value is not set. */
        public Value GetSpaceRightValue()
        {
            return spaceRight;
        }

        public float GetSpaceRight()
        {
            return spaceRight == null ? 0 : spaceRight.Get(element);
        }

        /** @return May be null if this value is not set. */
        public Value GetPadTopValue()
        {
            return padTop;
        }

        public float GetPadTop()
        {
            return padTop == null ? 0 : padTop.Get(element);
        }

        /** @return May be null if this value is not set. */
        public Value GetPadLeftValue()
        {
            return padLeft;
        }

        public float GetPadLeft()
        {
            return padLeft == null ? 0 : padLeft.Get(element);
        }

        /** @return May be null if this value is not set. */
        public Value GetPadBottomValue()
        {
            return padBottom;
        }

        public float GetPadBottom()
        {
            return padBottom == null ? 0 : padBottom.Get(element);
        }

        /** @return May be null if this value is not set. */
        public Value GetPadRightValue()
        {
            return padRight;
        }

        public float GetPadRight()
        {
            return padRight == null ? 0 : padRight.Get(element);
        }

        /** @return May be null if this value is not set. */
        public float? GetFillX()
        {
            return fillX;
        }

        /** @return May be null. */
        public float? GetFillY()
        {
            return fillY;
        }

        /** @return May be null. */
        public int? GetAlign()
        {
            return align;
        }

        /** @return May be null. */
        public int? GetExpandX()
        {
            return expandX;
        }

        /** @return May be null. */
        public int? GetExpandY()
        {
            return expandY;
        }

        /** @return May be null. */
        public int? GetColspan()
        {
            return colspan;
        }

        /** @return May be null. */
        public bool? GetUniformX()
        {
            return uniformX;
        }

        /** @return May be null. */
        public bool? GetUniformY()
        {
            return uniformY;
        }

        /** Returns true if this cell is the last cell in the row. */
        public bool IsEndRow()
        {
            return endRow;
        }

        /** The actual amount of combined padding and spacing from the last layout. */
        public float GetComputedPadTop()
        {
            return computedPadTop;
        }

        /** The actual amount of combined padding and spacing from the last layout. */
        public float GetComputedPadLeft()
        {
            return computedPadLeft;
        }

        /** The actual amount of combined padding and spacing from the last layout. */
        public float GetComputedPadBottom()
        {
            return computedPadBottom;
        }

        /** The actual amount of combined padding and spacing from the last layout. */
        public float GetComputedPadRight()
        {
            return computedPadRight;
        }

        public Cell Row()
        {
            return table.Row();
        }

        #endregion

        /// <summary>
		/// Returns the defaults to use for all cells. This can be used to avoid needing to set the same defaults for every table (eg,
		/// for spacing).
		/// </summary>
		/// <returns>The defaults.</returns>
		public static Cell GetDefaults()
        {
            if (!files)
            {
                files = true;
                defaults = new Cell
                {
                    minWidth = Value.minWidth,
                    minHeight = Value.minHeight,
                    prefWidth = Value.prefWidth,
                    prefHeight = Value.prefHeight,
                    maxWidth = Value.maxWidth,
                    maxHeight = Value.maxHeight,
                    spaceTop = Value.zero,
                    spaceLeft = Value.zero,
                    spaceBottom = Value.zero,
                    spaceRight = Value.zero,
                    padTop = Value.zero,
                    padLeft = Value.zero,
                    padBottom = Value.zero,
                    padRight = Value.zero,
                    fillX = 0f,
                    fillY = 0f,
                    align = AlignInternal.center,
                    expandX = 0,
                    expandY = 0,
                    colspan = 1,
                    uniformX = null,
                    uniformY = null
                };
            }
            return defaults;
        }


        /// <summary>
        /// Sets all constraint fields to null
        /// </summary>
        public void Clear()
        {
            minWidth = null;
            minHeight = null;
            prefWidth = null;
            prefHeight = null;
            maxWidth = null;
            maxHeight = null;
            spaceTop = null;
            spaceLeft = null;
            spaceBottom = null;
            spaceRight = null;
            padTop = null;
            padLeft = null;
            padBottom = null;
            padRight = null;
            fillX = null;
            fillY = null;
            align = null;
            expandX = null;
            expandY = null;
            colspan = null;
            uniformX = null;
            uniformY = null;
        }


        /// <summary>
        /// Reset state so the cell can be reused, setting all constraints to their {@link #defaults() default} values.
        /// </summary>
        public void Reset()
        {
            element = null;
            table = null;
            endRow = false;
            cellAboveIndex = -1;

            var defaults = GetDefaults();
            if (defaults != null)
                Set(defaults);
        }


        public void Set(Cell cell)
        {
            minWidth = cell.minWidth;
            minHeight = cell.minHeight;
            prefWidth = cell.prefWidth;
            prefHeight = cell.prefHeight;
            maxWidth = cell.maxWidth;
            maxHeight = cell.maxHeight;
            spaceTop = cell.spaceTop;
            spaceLeft = cell.spaceLeft;
            spaceBottom = cell.spaceBottom;
            spaceRight = cell.spaceRight;
            padTop = cell.padTop;
            padLeft = cell.padLeft;
            padBottom = cell.padBottom;
            padRight = cell.padRight;
            fillX = cell.fillX;
            fillY = cell.fillY;
            align = cell.align;
            expandX = cell.expandX;
            expandY = cell.expandY;
            colspan = cell.colspan;
            uniformX = cell.uniformX;
            uniformY = cell.uniformY;
        }


        /// <summary>
        /// cell may be null
        /// </summary>
        /// <param name="cell">Cell.</param>
        public void Merge(Cell cell)
        {
            if (cell == null)
                return;

            if (cell.minWidth != null)
                minWidth = cell.minWidth;
            if (cell.minHeight != null)
                minHeight = cell.minHeight;
            if (cell.prefWidth != null)
                prefWidth = cell.prefWidth;
            if (cell.prefHeight != null)
                prefHeight = cell.prefHeight;
            if (cell.maxWidth != null)
                maxWidth = cell.maxWidth;
            if (cell.maxHeight != null)
                maxHeight = cell.maxHeight;
            if (cell.spaceTop != null)
                spaceTop = cell.spaceTop;
            if (cell.spaceLeft != null)
                spaceLeft = cell.spaceLeft;
            if (cell.spaceBottom != null)
                spaceBottom = cell.spaceBottom;
            if (cell.spaceRight != null)
                spaceRight = cell.spaceRight;
            if (cell.padTop != null)
                padTop = cell.padTop;
            if (cell.padLeft != null)
                padLeft = cell.padLeft;
            if (cell.padBottom != null)
                padBottom = cell.padBottom;
            if (cell.padRight != null)
                padRight = cell.padRight;
            if (cell.fillX != null)
                fillX = cell.fillX;
            if (cell.fillY != null)
                fillY = cell.fillY;
            if (cell.align != null)
                align = cell.align;
            if (cell.expandX != null)
                expandX = cell.expandX;
            if (cell.expandY != null)
                expandY = cell.expandY;
            if (cell.colspan != null)
                colspan = cell.colspan;
            if (cell.uniformX != null)
                uniformX = cell.uniformX;
            if (cell.uniformY != null)
                uniformY = cell.uniformY;
        }


        public override string ToString()
        {
            return element != null ? element.ToString() : base.ToString();
        }
    }
}
