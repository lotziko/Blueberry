using System;
using System.Collections.Generic;
using System.Linq;

namespace Blueberry.UI
{
    public partial class Table : Group
    {
        #region Fields

        private int rows, columns;
        private bool implicitEndRow;
        private readonly List<Cell> cells = new List<Cell>(4);
        private readonly Cell cellDefaults;
        private readonly List<Cell> columnDefaults = new List<Cell>(2);
        private Cell rowDefaults;

        private bool sizeInvalid = true;
        private float[] columnMinWidth, rowMinHeight;
        private float[] columnPrefWidth, rowPrefHeight;
        private float tableMinWidth, tableMinHeight;
        private float tablePrefWidth, tablePrefHeight;
        private float[] columnWidth, rowHeight;
        private float[] expandWidth, expandHeight;
        private static float[] columnWeightedWidth, rowWeightedHeight;

        Value padTop = backgroundTop, padLeft = backgroundLeft, padBottom = backgroundBottom, padRight = backgroundRight;
        int align = AlignInternal.center;

        bool round = true, clip = false;
        Debug tableDebug = Debug.ALL;
        List<DebugRectangle> debugRects;

        protected IDrawable background;

        public static Col debugTableColor = new Col(0, 0, 255, 255);
        public static Col debugCellColor = new Col(255, 0, 0, 255);
        public static Col debugElementColor = new Col(0, 255, 0, 255);

        #endregion

        public Table()
        {
            cellDefaults = Pool<Cell>.Obtain();
            SetTransform(false);
            SetTouchable(Touchable.ChildrenOnly);
        }

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            Validate();
            if (transform)
            {
                ApplyTransform(graphics, ComputeTransform());
                DrawBackground(graphics, parentAlpha, 0, 0);

                if (clip)
                {
                    graphics.Flush();
                    float padLeft = this.padLeft.Get(this), padTop = this.padTop.Get(this);
                    if (ClipBegin(graphics, padLeft, padTop, GetWidth() - padLeft - padRight.Get(this), GetHeight() - padTop - padBottom.Get(this)))
                    {
                        DrawElements(graphics, parentAlpha);
                        ClipEnd(graphics);
                    }
                }
                else
                {
                    DrawElements(graphics, parentAlpha);
                }
                ResetTransform(graphics);
            }
            else
            {
                DrawBackground(graphics, parentAlpha, x, y);
                base.Draw(graphics, parentAlpha);
            }
        }

        public override void DrawDebug(Graphics graphics)
        {
            if (debugRects != null)
            {
                foreach (var d in debugRects)
                    graphics.DrawRectangle(x + d.rectange.x, y + d.rectange.y, d.rectange.width, d.rectange.height, true, d.color);
            }

            base.DrawDebug(graphics);
        }

        protected virtual void DrawBackground(Graphics graphics, float parentAlpha, float x, float y)
        {
            if (background == null)
                return;

            background.Draw(graphics, x, y, width, height, new Col(color, color.A * parentAlpha));
            graphics.Flush();
        }

        #region Getters

        public Debug GetTableDebug()
        {
            return tableDebug;
        }

        public Value GetPadTopValue()
        {
            return padTop;
        }

        public float GetPadTop()
        {
            return padTop.Get(this);
        }

        public Value GetPadLeftValue()
        {
            return padLeft;
        }

        public float GetPadLeft()
        {
            return padLeft.Get(this);
        }

        public Value GetPadBottomValue()
        {
            return padBottom;
        }

        public float GetPadBottom()
        {
            return padBottom.Get(this);
        }

        public Value GetPadRightValue()
        {
            return padRight;
        }

        public float GetPadRight()
        {
            return padRight.Get(this);
        }

        /** Returns {@link #getPadLeft()} plus {@link #getPadRight()}. */
        public float GetPadX()
        {
            return padLeft.Get(this) + padRight.Get(this);
        }

        /** Returns {@link #getPadTop()} plus {@link #getPadBottom()}. */
        public float GetPadY()
        {
            return padTop.Get(this) + padBottom.Get(this);
        }

        public int GetAlign()
        {
            return align;
        }

        public bool GetClip()
        {
            return clip;
        }

        #endregion

        public override Element Hit(float x, float y, bool touchable)
        {
            if (clip)
            {
                if (touchable && GetTouchable() == Touchable.Disabled) return null;
                if (x < 0 || x >= GetWidth() || y < 0 || y >= GetHeight()) return null;
            }
            return base.Hit(x, y, touchable);
        }

        public virtual void SetClip(bool enabled)
        {
            clip = enabled;
            SetTransform(enabled);
            Invalidate();
        }

        #region ILayout

        public override void Invalidate()
        {
            sizeInvalid = true;
        }

        public override float MinWidth
        {
            get
            {
                if (sizeInvalid)
                    ComputeSize();
                return tableMinWidth;
            }
        }

        public override float MinHeight
        {
            get
            {
                if (sizeInvalid)
                    ComputeSize();
                return tableMinHeight;
            }
        }

        public override float PreferredWidth
        {
            get
            {
                if (sizeInvalid)
                    ComputeSize();
                var width = tablePrefWidth;
                if (background != null)
                    return Math.Max(width, background.MinWidth);
                return width;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                if (sizeInvalid)
                    ComputeSize();
                var height = tablePrefHeight;
                if (background != null)
                    return Math.Max(height, background.MinHeight);
                return height;
            }
        }

        #endregion

        #region Size calculating

        void ComputeSize()
        {
            sizeInvalid = false;

            var cellCount = cells.Count;

            // Implicitly End the row for layout purposes.
            if (cellCount > 0 && !cells.Last().endRow)
            {
                EndRow();
                implicitEndRow = true;
            }
            else
            {
                implicitEndRow = false;
            }

            int columns = this.columns, rows = this.rows;
            columnWidth = EnsureSize(columnWidth, columns);
            rowHeight = EnsureSize(rowHeight, rows);
            columnMinWidth = EnsureSize(columnMinWidth, columns);
            rowMinHeight = EnsureSize(rowMinHeight, rows);
            columnPrefWidth = EnsureSize(columnPrefWidth, columns);
            rowPrefHeight = EnsureSize(rowPrefHeight, rows);
            expandWidth = EnsureSize(expandWidth, columns);
            expandHeight = EnsureSize(expandHeight, rows);

            var spaceRightLast = 0f;
            for (var i = 0; i < cellCount; i++)
            {
                var cell = cells[i];
                int column = cell.column, row = cell.row, colspan = cell.colspan.Value;

                // Collect rows that expand and colspan=1 columns that expand.
                if (cell.expandY != 0 && expandHeight[row] == 0)
                    expandHeight[row] = cell.expandY.Value;
                if (colspan == 1 && cell.expandX != 0 && expandWidth[column] == 0)
                    expandWidth[column] = cell.expandX.Value;

                // Compute combined padding/spacing for cells.
                // Spacing between elements isn't additive, the larger is used. Also, no spacing around edges.
                cell.computedPadLeft = cell.padLeft.Get(cell.element) + (column == 0 ? 0 : Math.Max(0, cell.spaceLeft.Get(cell.element) - spaceRightLast));
                cell.computedPadTop = cell.padTop.Get(cell.element);
                if (cell.cellAboveIndex != -1)
                {
                    var above = cells[cell.cellAboveIndex];
                    cell.computedPadTop += Math.Max(0, cell.spaceTop.Get(cell.element) - above.spaceBottom.Get(cell.element));
                }

                var spaceRight = cell.spaceRight.Get(cell.element);
                cell.computedPadRight = cell.padRight.Get(cell.element) + ((column + colspan) == columns ? 0 : spaceRight);
                cell.computedPadBottom = cell.padBottom.Get(cell.element) + (row == rows - 1 ? 0 : cell.spaceBottom.Get(cell.element));
                spaceRightLast = spaceRight;

                // Determine minimum and preferred cell sizes.
                var prefWidth = cell.prefWidth.Get(cell.element);
                var prefHeight = cell.prefHeight.Get(cell.element);
                var minWidth = cell.minWidth.Get(cell.element);
                var minHeight = cell.minHeight.Get(cell.element);
                var maxWidth = cell.maxWidth.Get(cell.element);
                var maxHeight = cell.maxHeight.Get(cell.element);

                if (prefWidth < minWidth)
                    prefWidth = minWidth;
                if (prefHeight < minHeight)
                    prefHeight = minHeight;
                if (maxWidth > 0 && prefWidth > maxWidth)
                    prefWidth = maxWidth;
                if (maxHeight > 0 && prefHeight > maxHeight)
                    prefHeight = maxHeight;

                if (colspan == 1)
                {
                    // Spanned column min and pref width is added later.
                    var hpadding = cell.computedPadLeft + cell.computedPadRight;
                    columnPrefWidth[column] = Math.Max(columnPrefWidth[column], prefWidth + hpadding);
                    columnMinWidth[column] = Math.Max(columnMinWidth[column], minWidth + hpadding);
                }
                float vpadding = cell.computedPadTop + cell.computedPadBottom;
                rowPrefHeight[row] = Math.Max(rowPrefHeight[row], prefHeight + vpadding);
                rowMinHeight[row] = Math.Max(rowMinHeight[row], minHeight + vpadding);
            }

            float uniformMinWidth = 0, uniformMinHeight = 0;
            float uniformPrefWidth = 0, uniformPrefHeight = 0;
            for (var i = 0; i < cellCount; i++)
            {
                var c = cells[i];

                // Colspan with expand will expand all spanned columns if none of the spanned columns have expand.
                var expandX = c.expandX.Value;
                if (expandX != 0)
                {
                    int nn = c.column + c.colspan.Value;
                    for (int ii = c.column; ii < nn; ii++)
                        if (expandWidth[ii] != 0)
                            goto outer;
                    for (int ii = c.column; ii < nn; ii++)
                        expandWidth[ii] = expandX;
                }
            outer:
                { }

                // Collect uniform sizes.
                if (c.uniformX.HasValue && c.uniformX.Value && c.colspan == 1)
                {
                    float hpadding = c.computedPadLeft + c.computedPadRight;
                    uniformMinWidth = Math.Max(uniformMinWidth, columnMinWidth[c.column] - hpadding);
                    uniformPrefWidth = Math.Max(uniformPrefWidth, columnPrefWidth[c.column] - hpadding);
                }

                if (c.uniformY.HasValue && c.uniformY.Value)
                {
                    float vpadding = c.computedPadTop + c.computedPadBottom;
                    uniformMinHeight = Math.Max(uniformMinHeight, rowMinHeight[c.row] - vpadding);
                    uniformPrefHeight = Math.Max(uniformPrefHeight, rowPrefHeight[c.row] - vpadding);
                }
            }

            // Size uniform cells to the same width/height.
            if (uniformPrefWidth > 0 || uniformPrefHeight > 0)
            {
                for (var i = 0; i < cellCount; i++)
                {
                    var c = cells[i];
                    if (uniformPrefWidth > 0 && c.uniformX.HasValue && c.uniformX.Value && c.colspan == 1)
                    {
                        var hpadding = c.computedPadLeft + c.computedPadRight;
                        columnMinWidth[c.column] = uniformMinWidth + hpadding;
                        columnPrefWidth[c.column] = uniformPrefWidth + hpadding;
                    }

                    if (uniformPrefHeight > 0 && c.uniformY.HasValue && c.uniformY.Value)
                    {
                        var vpadding = c.computedPadTop + c.computedPadBottom;
                        rowMinHeight[c.row] = uniformMinHeight + vpadding;
                        rowPrefHeight[c.row] = uniformPrefHeight + vpadding;
                    }
                }
            }

            // Distribute any additional min and pref width added by colspanned cells to the columns spanned.
            for (var i = 0; i < cellCount; i++)
            {
                var c = cells[i];
                var colspan = c.colspan.Value;
                if (colspan == 1)
                    continue;

                var a = c.element;
                var minWidth = c.minWidth.Get(a);
                var prefWidth = c.prefWidth.Get(a);
                var maxWidth = c.maxWidth.Get(a);
                if (prefWidth < minWidth)
                    prefWidth = minWidth;
                if (maxWidth > 0 && prefWidth > maxWidth)
                    prefWidth = maxWidth;

                float spannedMinWidth = -(c.computedPadLeft + c.computedPadRight), spannedPrefWidth = spannedMinWidth;
                var totalExpandWidth = 0f;
                for (int ii = c.column, nn = ii + colspan; ii < nn; ii++)
                {
                    spannedMinWidth += columnMinWidth[ii];
                    spannedPrefWidth += columnPrefWidth[ii];
                    totalExpandWidth += expandWidth[ii]; // Distribute extra space using expand, if any columns have expand.
                }

                var extraMinWidth = Math.Max(0, minWidth - spannedMinWidth);
                var extraPrefWidth = Math.Max(0, prefWidth - spannedPrefWidth);
                for (int ii = c.column, nn = ii + colspan; ii < nn; ii++)
                {
                    float ratio = totalExpandWidth == 0 ? 1f / colspan : expandWidth[ii] / totalExpandWidth;
                    columnMinWidth[ii] += extraMinWidth * ratio;
                    columnPrefWidth[ii] += extraPrefWidth * ratio;
                }
            }

            // Determine table min and pref size.
            tableMinWidth = 0;
            tableMinHeight = 0;
            tablePrefWidth = 0;
            tablePrefHeight = 0;
            for (var i = 0; i < columns; i++)
            {
                tableMinWidth += columnMinWidth[i];
                tablePrefWidth += columnPrefWidth[i];
            }

            for (var i = 0; i < rows; i++)
            {
                tableMinHeight += rowMinHeight[i];
                tablePrefHeight += Math.Max(rowMinHeight[i], rowPrefHeight[i]);
            }

            var hpadding_ = padLeft.Get(this) + padRight.Get(this);
            var vpadding_ = padTop.Get(this) + padBottom.Get(this);
            tableMinWidth = tableMinWidth + hpadding_;
            tableMinHeight = tableMinHeight + vpadding_;
            tablePrefWidth = Math.Max(tablePrefWidth + hpadding_, tableMinWidth);
            tablePrefHeight = Math.Max(tablePrefHeight + vpadding_, tableMinHeight);
        }

        public override void Layout()
        {
            Layout(0, 0, width, height);

            if (round)
            {
                for (int i = 0, n = cells.Count; i < n; i++)
                {
                    var c = cells[i];
                    var elementWidth = (float)Math.Round(c.elementWidth);
                    var elementHeight = (float)Math.Round(c.elementHeight);
                    var elementX = (float)Math.Round(c.elementX);
                    var elementY = (float)Math.Round(c.elementY);
                    c.SetElementBounds(elementX, elementY, elementWidth, elementHeight);

                    if (c.element != null)
                        c.element.SetBounds(elementX, elementY, elementWidth, elementHeight);
                }
            }
            else
            {
                for (int i = 0, n = cells.Count; i < n; i++)
                {
                    var c = cells[i];
                    var elementY = c.elementY;
                    c.SetElementY(elementY);

                    if (c.element != null)
                        c.element.SetBounds(c.elementX, elementY, c.elementWidth, c.elementHeight);
                }
            }

            // Validate children separately from sizing elements to ensure elements without a cell are validated.
            for (int i = 0, n = elements.Count; i < n; i++)
            {
                var child = elements[i];
                if (child is ILayout)
                    ((ILayout)child).Validate();
            }
        }

        void Layout(float layoutX, float layoutY, float layoutWidth, float layoutHeight)
        {
            if (sizeInvalid)
                ComputeSize();

            var cellCount = cells.Count;
            var padLeft = this.padLeft.Get(this);
            var hpadding = padLeft + padRight.Get(this);
            var padTop = this.padTop.Get(this);
            var vpadding = padTop + padBottom.Get(this);

            int columns = this.columns, rows = this.rows;
            float[] expandWidth = this.expandWidth, expandHeight = this.expandHeight;
            float[] columnWidth = this.columnWidth, rowHeight = this.rowHeight;

            float totalExpandWidth = 0, totalExpandHeight = 0;
            for (var i = 0; i < columns; i++)
                totalExpandWidth += expandWidth[i];
            for (var i = 0; i < rows; i++)
                totalExpandHeight += expandHeight[i];

            // Size columns and rows between min and pref size using (preferred - min) size to weight distribution of extra space.
            float[] columnWeightedWidth;
            float totalGrowWidth = tablePrefWidth - tableMinWidth;
            if (totalGrowWidth == 0)
            {
                columnWeightedWidth = columnMinWidth;
            }
            else
            {
                var extraWidth = Math.Min(totalGrowWidth, Math.Max(0, layoutWidth - tableMinWidth));
                columnWeightedWidth = Table.columnWeightedWidth = EnsureSize(Table.columnWeightedWidth, columns);
                float[] columnMinWidth = this.columnMinWidth, columnPrefWidth = this.columnPrefWidth;
                for (var i = 0; i < columns; i++)
                {
                    var growWidth = columnPrefWidth[i] - columnMinWidth[i];
                    var growRatio = growWidth / totalGrowWidth;
                    columnWeightedWidth[i] = columnMinWidth[i] + extraWidth * growRatio;
                }
            }

            float[] rowWeightedHeight;
            var totalGrowHeight = tablePrefHeight - tableMinHeight;
            if (totalGrowHeight == 0)
            {
                rowWeightedHeight = rowMinHeight;
            }
            else
            {
                rowWeightedHeight = Table.rowWeightedHeight = EnsureSize(Table.rowWeightedHeight, rows);
                var extraHeight = Math.Min(totalGrowHeight, Math.Max(0, layoutHeight - tableMinHeight));
                float[] rowMinHeight = this.rowMinHeight, rowPrefHeight = this.rowPrefHeight;
                for (int i = 0; i < rows; i++)
                {
                    float growHeight = rowPrefHeight[i] - rowMinHeight[i];
                    float growRatio = growHeight / totalGrowHeight;
                    rowWeightedHeight[i] = rowMinHeight[i] + extraHeight * growRatio;
                }
            }

            // Determine element and cell sizes (before expand or fill).
            for (var i = 0; i < cellCount; i++)
            {
                var cell = cells[i];
                int column = cell.column, row = cell.row;

                var spannedWeightedWidth = 0f;
                var colspan = cell.colspan.Value;
                for (int ii = column, nn = ii + colspan; ii < nn; ii++)
                    spannedWeightedWidth += columnWeightedWidth[ii];
                var weightedHeight = rowWeightedHeight[row];

                var prefWidth = cell.prefWidth.Get(cell.element);
                var prefHeight = cell.prefHeight.Get(cell.element);
                var minWidth = cell.minWidth.Get(cell.element);
                var minHeight = cell.minHeight.Get(cell.element);
                var maxWidth = cell.maxWidth.Get(cell.element);
                var maxHeight = cell.maxHeight.Get(cell.element);

                if (prefWidth < minWidth)
                    prefWidth = minWidth;
                if (prefHeight < minHeight)
                    prefHeight = minHeight;
                if (maxWidth > 0 && prefWidth > maxWidth)
                    prefWidth = maxWidth;
                if (maxHeight > 0 && prefHeight > maxHeight)
                    prefHeight = maxHeight;

                cell.elementWidth = Math.Min(spannedWeightedWidth - cell.computedPadLeft - cell.computedPadRight, prefWidth);
                cell.elementHeight = Math.Min(weightedHeight - cell.computedPadTop - cell.computedPadBottom, prefHeight);

                if (colspan == 1)
                    columnWidth[column] = Math.Max(columnWidth[column], spannedWeightedWidth);
                rowHeight[row] = Math.Max(rowHeight[row], weightedHeight);
            }

            // distribute remaining space to any expanding columns/rows.
            if (totalExpandWidth > 0)
            {
                var extra = layoutWidth - hpadding;
                for (var i = 0; i < columns; i++)
                    extra -= columnWidth[i];

                var used = 0f;
                var lastIndex = 0;
                for (var i = 0; i < columns; i++)
                {
                    if (expandWidth[i] == 0)
                        continue;
                    var amount = extra * expandWidth[i] / totalExpandWidth;
                    columnWidth[i] += amount;
                    used += amount;
                    lastIndex = i;
                }
                columnWidth[lastIndex] += extra - used;
            }

            if (totalExpandHeight > 0)
            {
                var extra = layoutHeight - vpadding;
                for (var i = 0; i < rows; i++)
                    extra -= rowHeight[i];

                var used = 0f;
                var lastIndex = 0;
                for (var i = 0; i < rows; i++)
                {
                    if (expandHeight[i] == 0)
                        continue;

                    var amount = extra * expandHeight[i] / totalExpandHeight;
                    rowHeight[i] += amount;
                    used += amount;
                    lastIndex = i;
                }
                rowHeight[lastIndex] += extra - used;
            }

            // distribute any additional width added by colspanned cells to the columns spanned.
            for (var i = 0; i < cellCount; i++)
            {
                var c = this.cells[i];
                var colspan = c.colspan.Value;
                if (colspan == 1)
                    continue;

                var extraWidth = 0f;
                for (int column = c.column, nn = column + colspan; column < nn; column++)
                    extraWidth += columnWeightedWidth[column] - columnWidth[column];
                extraWidth -= Math.Max(0, c.computedPadLeft + c.computedPadRight);

                extraWidth /= colspan;
                if (extraWidth > 0)
                {
                    for (int column = c.column, nn = column + colspan; column < nn; column++)
                        columnWidth[column] += extraWidth;
                }
            }

            // Determine table size.
            float tableWidth = hpadding, tableHeight = vpadding;
            for (var i = 0; i < columns; i++)
                tableWidth += columnWidth[i];
            for (var i = 0; i < rows; i++)
                tableHeight += rowHeight[i];

            // Position table within the container.
            var x = layoutX + padLeft;
            if ((this.align & AlignInternal.right) != 0)
                x += layoutWidth - tableWidth;
            else if ((this.align & AlignInternal.left) == 0) // Center
                x += (layoutWidth - tableWidth) / 2;

            var y = layoutY + padTop; // bottom
            if ((this.align & AlignInternal.bottom) != 0)
                y += layoutHeight - tableHeight;
            else if ((this.align & AlignInternal.top) == 0) // Center
                y += (layoutHeight - tableHeight) / 2;

            // position elements within cells.
            float currentX = x, currentY = y;
            for (var i = 0; i < cellCount; i++)
            {
                var c = this.cells[i];

                var spannedCellWidth = 0f;
                for (int column = c.column, nn = column + c.colspan.Value; column < nn; column++)
                    spannedCellWidth += columnWidth[column];
                spannedCellWidth -= c.computedPadLeft + c.computedPadRight;

                currentX += c.computedPadLeft;

                float fillX = c.fillX.Value, fillY = c.fillY.Value;
                if (fillX > 0)
                {
                    c.elementWidth = Math.Max(spannedCellWidth * fillX, c.minWidth.Get(c.element));
                    var maxWidth = c.maxWidth.Get(c.element);
                    if (maxWidth > 0)
                        c.elementWidth = Math.Min(c.elementWidth, maxWidth);
                }
                if (fillY > 0)
                {
                    c.elementHeight = Math.Max(rowHeight[c.row] * fillY - c.computedPadTop - c.computedPadBottom, c.minHeight.Get(c.element));
                    var maxHeight = c.maxHeight.Get(c.element);
                    if (maxHeight > 0)
                        c.elementHeight = Math.Min(c.elementHeight, maxHeight);
                }

                var cellAlign = c.align.Value;
                if ((cellAlign & AlignInternal.left) != 0)
                    c.elementX = currentX;
                else if ((cellAlign & AlignInternal.right) != 0)
                    c.elementX = currentX + spannedCellWidth - c.elementWidth;
                else
                    c.elementX = currentX + (spannedCellWidth - c.elementWidth) / 2;

                if ((cellAlign & AlignInternal.top) != 0)
                    c.elementY = currentY + c.computedPadTop;
                else if ((cellAlign & AlignInternal.bottom) != 0)
                    c.elementY = currentY + rowHeight[c.row] - c.elementHeight - c.computedPadBottom;
                else
                    c.elementY = currentY + (rowHeight[c.row] - c.elementHeight + c.computedPadTop - c.computedPadBottom) / 2;

                if (c.endRow)
                {
                    currentX = x;
                    currentY += rowHeight[c.row];
                }
                else
                {
                    currentX += spannedCellWidth + c.computedPadRight;
                }
            }

            if (tableDebug != Debug.NONE)
                ComputeDebugRects(x, y, layoutX, layoutY, layoutWidth, layoutHeight, tableWidth, tableHeight, hpadding, vpadding);
        }

        float[] EnsureSize(float[] array, int size)
        {
            if (array == null || array.Length < size)
                return new float[size];

            for (int i = 0, n = array.Length; i < n; i++)
                array[i] = 0;

            return array;
        }

        #endregion

        #region Cells

        /** The cell values that will be used as the defaults for all cells. */
        public Cell Defaults()
        {
            return cellDefaults;
        }

        public virtual Cell Add(Element element)
        {
            var cell = Pool<Cell>.Obtain();
            cell.element = element;
            cell.SetLayout(this);

            // the row was ended for layout, not by the user, so revert it.
            if (implicitEndRow)
            {
                implicitEndRow = false;
                rows--;
                cells.Last().endRow = false;
            }

            var cellCount = cells.Count;
            if (cellCount > 0)
            {
                // Set cell column and row.
                var lastCell = cells.Last();
                if (!lastCell.endRow)
                {
                    cell.column = lastCell.column + lastCell.colspan.Value;
                    cell.row = lastCell.row;
                }
                else
                {
                    cell.column = 0;
                    cell.row = lastCell.row + 1;
                }

                // set the index of the cell above.
                if (cell.row > 0)
                {
                    for (var i = cellCount - 1; i >= 0; i--)
                    {
                        var other = cells[i];
                        for (int column = other.column, nn = column + other.colspan.Value; column < nn; column++)
                        {
                            if (column == cell.column)
                            {
                                cell.cellAboveIndex = i;
                                goto outer;
                            }
                        }
                    }
                outer:
                    { }
                }
            }
            else
            {
                cell.column = 0;
                cell.row = 0;
            }
            cells.Add(cell);

            cell.Set(cellDefaults);
            if (cell.column < columnDefaults.Count)
            {
                var columnCell = columnDefaults[cell.column];
                if (columnCell != null)
                    cell.Merge(columnCell);
            }
            cell.Merge(rowDefaults);

            if (element != null)
                AddElement(element);

            return cell;
        }

        public Cell GetCell(Element element)
        {
            for (int i = 0, n = cells.Count; i < n; i++)
            {
                var c = cells[i];
                if (c.element == element)
                    return c;
            }
            return null;
        }

        public override bool RemoveElement(Element element)
        {
            if (!base.RemoveElement(element))
                return false;

            var cell = GetCell(element);
            if (cell != null)
                cell.element = null;
            return true;
        }

        /// <summary>
		/// Removes all elements and cells from the table
		/// </summary>
		public override void ClearElements()
        {
            for (int i = cells.Count - 1; i >= 0; i--)
            {
                var cell = cells[i];
                var element = cell.element;
                if (element != null)
                    element.Remove();

                Pool<Cell>.Free(cell);
            }

            cells.Clear();
            rows = 0;
            columns = 0;

            if (rowDefaults != null)
                Pool<Cell>.Free(rowDefaults);

            rowDefaults = null;
            implicitEndRow = false;

            base.ClearElements();
        }

        /// <summary>
		/// Removes all elements and cells from the table (same as {@link #clear()}) and additionally resets all table properties and
		/// cell, column, and row defaults.
		/// </summary>
		public void Reset()
        {
            Clear();
            padTop = backgroundTop;
            padLeft = backgroundLeft;
            padBottom = backgroundBottom;
            padRight = backgroundRight;
            align = AlignInternal.center;
            tableDebug = Debug.NONE;

            cellDefaults.Reset();

            for (int i = 0, n = columnDefaults.Count; i < n; i++)
            {
                var columnCell = columnDefaults[i];
                if (columnCell != null)
                    Pool<Cell>.Free(columnCell);
            }
            columnDefaults.Clear();
        }

        /// <summary>
		/// Indicates that subsequent cells should be added to a new row and returns the cell values that will be used as the defaults
		/// for all cells in the new row.
		/// </summary>
		public Cell Row()
        {
            if (cells.Count > 0)
            {
                EndRow();
                Invalidate();
            }

            implicitEndRow = false;
            if (rowDefaults != null)
                Pool<Cell>.Free(rowDefaults);

            rowDefaults = Pool<Cell>.Obtain();
            rowDefaults.Clear();
            return rowDefaults;
        }

        void EndRow()
        {
            var rowColumns = 0;
            for (var i = cells.Count - 1; i >= 0; i--)
            {
                var cell = cells[i];
                if (cell.endRow)
                    break;

                rowColumns += cell.colspan.Value;
            }

            columns = Math.Max(columns, rowColumns);
            rows++;
            cells.Last().endRow = true;
        }

        /// <summary>
		/// Gets the cell values that will be used as the defaults for all cells in the specified column. Columns are indexed starting at 0
		/// </summary>
		/// <returns>The column defaults.</returns>
		/// <param name="column">Column.</param>
		public Cell GetColumnDefaults(int column)
        {
            var cell = columnDefaults.Count > column ? columnDefaults[column] : null;
            if (cell == null)
            {
                cell = Pool<Cell>.Obtain();
                cell.SetLayout(this);
                cell.Clear();
                if (column >= columnDefaults.Count)
                {
                    for (int i = columnDefaults.Count; i < column; i++)
                        columnDefaults.Add(null);
                    columnDefaults.Add(cell);
                }
                else
                {
                    columnDefaults[column] = cell;
                }
            }
            return cell;
        }

        #endregion

        #region Align

        /// <summary>
        /// Alignment of the logical table within the table element. Set to {@link Align#center}, {@link Align#top}, {@link Align#bottom}
        /// {@link Align#left}, {@link Align#right}, or any combination of those.
        /// </summary>
        /// <param name="align">Align.</param>
        public Table Align(int align)
        {
            this.align = align;
            return this;
        }


        /// <summary>
        /// Sets the alignment of the logical table within the table element to {@link Align#center}. This clears any other alignment.
        /// </summary>
        public Table Center()
        {
            align = AlignInternal.center;
            return this;
        }


        /// <summary>
        /// Adds {@link Align#top} and clears {@link Align#bottom} for the alignment of the logical table within the table element.
        /// </summary>
        public Table Top()
        {
            align |= AlignInternal.top;
            align &= ~AlignInternal.bottom;
            return this;
        }


        /// <summary>
        /// Adds {@link Align#left} and clears {@link Align#right} for the alignment of the logical table within the table element.
        /// </summary>
        public Table Left()
        {
            align |= AlignInternal.left;
            align &= ~AlignInternal.right;
            return this;
        }


        /// <summary>
        /// Adds {@link Align#bottom} and clears {@link Align#top} for the alignment of the logical table within the table element.
        /// </summary>
        public Table Bottom()
        {
            align |= AlignInternal.bottom;
            align &= ~AlignInternal.top;
            return this;
        }


        /// <summary>
        /// Adds {@link Align#right} and clears {@link Align#left} for the alignment of the logical table within the table element.
        /// </summary>
        public Table Right()
        {
            align |= AlignInternal.right;
            align &= ~AlignInternal.left;
            return this;
        }

        #endregion

        #region Pad

        /** Sets the padTop, padLeft, padBottom, and padRight around the table to the specified value. */
        public Table Pad(Value pad)
        {
            padTop = pad ?? throw new ArgumentNullException("pad cannot be null.");
            padLeft = pad;
            padBottom = pad;
            padRight = pad;
            sizeInvalid = true;
            return this;
        }

        public Table Pad(Value top, Value left, Value bottom, Value right)
        {
            padTop = top ?? throw new ArgumentNullException("top cannot be null.");
            padLeft = left ?? throw new ArgumentNullException("left cannot be null.");
            padBottom = bottom ?? throw new ArgumentNullException("bottom cannot be null.");
            padRight = right ?? throw new ArgumentNullException("right cannot be null.");
            sizeInvalid = true;
            return this;
        }

        /** Padding at the top edge of the table. */
        public Table PadTop(Value padTop)
        {
            this.padTop = padTop ?? throw new ArgumentNullException("padTop cannot be null.");
            sizeInvalid = true;
            return this;
        }

        /** Padding at the left edge of the table. */
        public Table PadLeft(Value padLeft)
        {
            this.padLeft = padLeft ?? throw new ArgumentNullException("padLeft cannot be null.");
            sizeInvalid = true;
            return this;
        }

        /** Padding at the bottom edge of the table. */
        public Table PadBottom(Value padBottom)
        {
            this.padBottom = padBottom ?? throw new ArgumentNullException("padBottom cannot be null.");
            sizeInvalid = true;
            return this;
        }

        /** Padding at the right edge of the table. */
        public Table PadRight(Value padRight)
        {
            this.padRight = padRight ?? throw new ArgumentNullException("padRight cannot be null.");
            sizeInvalid = true;
            return this;
        }

        /** Sets the padTop, padLeft, padBottom, and padRight around the table to the specified value. */
        public Table Pad(float pad)
        {
            Pad(new Value.Fixed(pad));
            return this;
        }

        public Table Pad(float top, float left, float bottom, float right)
        {
            padTop = new Value.Fixed(top);
            padLeft = new Value.Fixed(left);
            padBottom = new Value.Fixed(bottom);
            padRight = new Value.Fixed(right);
            sizeInvalid = true;
            return this;
        }

        /** Padding at the top edge of the table. */
        public Table PadTop(float padTop)
        {
            this.padTop = new Value.Fixed(padTop);
            sizeInvalid = true;
            return this;
        }

        /** Padding at the left edge of the table. */
        public Table PadLeft(float padLeft)
        {
            this.padLeft = new Value.Fixed(padLeft);
            sizeInvalid = true;
            return this;
        }

        /** Padding at the bottom edge of the table. */
        public Table PadBottom(float padBottom)
        {
            this.padBottom = new Value.Fixed(padBottom);
            sizeInvalid = true;
            return this;
        }

        /** Padding at the right edge of the table. */
        public Table PadRight(float padRight)
        {
            this.padRight = new Value.Fixed(padRight);
            sizeInvalid = true;
            return this;
        }

        #endregion

        public void SetBackground(IDrawable background)
        {
            if (this.background == background) return;
            float padTopOld = GetPadTop(), padLeftOld = GetPadLeft(), padBottomOld = GetPadBottom(), padRightOld = GetPadRight();
            this.background = background; // The default pad values use the background's padding.
            float padTopNew = GetPadTop(), padLeftNew = GetPadLeft(), padBottomNew = GetPadBottom(), padRightNew = GetPadRight();
            if (padTopOld + padBottomOld != padTopNew + padBottomNew || padLeftOld + padRightOld != padLeftNew + padRightNew)
                InvalidateHierarchy();
            else if (padTopOld != padTopNew || padLeftOld != padLeftNew || padBottomOld != padBottomNew || padRightOld != padRightNew)
                Invalidate();
        }

        public IDrawable GetBackground()
        {
            return background;
        }

        #region Value types

        static public Value backgroundTop = new BackgroundTopValue();

        /// <summary>
        /// Value that is the top padding of the table's background
        /// </summary>
        public class BackgroundTopValue : Value
        {
            public override float Get(Element context)
            {
                var background = ((Table)context).background;
                return background == null ? 0 : background.TopHeight;
            }
        }


        static public Value backgroundLeft = new BackgroundLeftValue();

        /// <summary>
        /// Value that is the left padding of the table's background
        /// </summary>
        public class BackgroundLeftValue : Value
        {
            public override float Get(Element context)
            {
                var background = ((Table)context).background;
                return background == null ? 0 : background.LeftWidth;
            }
        }


        static public Value backgroundBottom = new BackgroundBottomValue();

        /// <summary>
        /// Value that is the bottom padding of the table's background
        /// </summary>
        public class BackgroundBottomValue : Value
        {
            public override float Get(Element context)
            {
                var background = ((Table)context).background;
                return background == null ? 0 : background.BottomHeight;
            }
        }


        static public Value backgroundRight = new BackgroundRightValue();

        /// <summary>
        /// Value that is the right padding of the table's background
        /// </summary>
        public class BackgroundRightValue : Value
        {
            public override float Get(Element context)
            {
                var background = ((Table)context).background;
                return background == null ? 0 : background.RightWidth;
            }
        }

        #endregion

        #region Debug

        void ComputeDebugRects(float x, float y, float layoutX, float layoutY, float layoutWidth, float layoutHeight, float tableWidth, float tableHeight, float hpadding, float vpadding)
        {
            if (debugRects != null)
                debugRects.Clear();

            var currentX = x;
            var currentY = y;
            if (tableDebug == Debug.TABLE || tableDebug == Debug.ALL)
            {
                AddDebugRect(layoutX, layoutY, layoutWidth, layoutHeight, debugTableColor);
                AddDebugRect(x, y, tableWidth - hpadding, tableHeight - vpadding, debugTableColor);
            }

            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];

                // element bounds.
                if (tableDebug == Debug.ELEMENT || tableDebug == Debug.ALL)
                    AddDebugRect(cell.elementX, cell.elementY, cell.elementWidth, cell.elementHeight, debugElementColor);

                // Cell bounds.
                float spannedCellWidth = 0;
                for (int column = cell.column, nn = column + cell.colspan.Value; column < nn; column++)
                    spannedCellWidth += columnWidth[column];
                spannedCellWidth -= cell.computedPadLeft + cell.computedPadRight;
                currentX += cell.computedPadLeft;

                if (tableDebug == Debug.CELL || tableDebug == Debug.ALL)
                {
                    AddDebugRect(currentX, currentY + cell.computedPadTop, spannedCellWidth, rowHeight[cell.row] - cell.computedPadTop - cell.computedPadBottom, debugCellColor);
                }

                if (cell.endRow)
                {
                    currentX = x;
                    currentY += rowHeight[cell.row];
                }
                else
                {
                    currentX += spannedCellWidth + cell.computedPadRight;
                }
            }
        }

        void AddDebugRect(float x, float y, float w, float h, Col color)
        {
            if (debugRects == null)
                debugRects = new List<DebugRectangle>();

            var rect = new DebugRectangle(x, y, w, h, color);
            debugRects.Add(rect);
        }

        #endregion
    }

    public struct DebugRectangle
    {
        public (float x, float y, float width, float height) rectange;
        public Col color;

        public DebugRectangle(float x, float y, float w, float h, Col color)
        {
            rectange = (x, y, w, h);
            this.color = color;
        }
    }

    public enum Debug
    {
        NONE, ALL, TABLE, CELL, ELEMENT
    }

    public class ButtonStyle
    {
        public IDrawable up, down, over, checkked, checkedOver, disabled, focusedBorder;

        public ButtonStyle()
        {

        }

        public ButtonStyle(IDrawable up, IDrawable down, IDrawable checkked)
        {
            this.up = up;
            this.down = down;
            this.checkked = checkked;
        }

        public ButtonStyle(ButtonStyle style)
        {
            this.up = style.up;
            this.down = style.down;
            this.over = style.over;
            this.checkedOver = style.checkedOver;
            this.disabled = style.disabled;
        }
    }
}
