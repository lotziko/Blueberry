using System;
using System.Collections.Generic;
using BlueberryCore.SMath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BlueberryCore.UI
{
    public class TextArea : TextField
    {
        /** Array storing lines breaks positions * */
        protected List<int> linesBreak = new List<int>();

        /** Last text processed. This attribute is used to avoid unnecessary computations while calculating offsets * */
        private String lastText;

        /** Current line for the cursor * */
        int cursorLine;

        /** Index of the first line showed by the text area * */
        int firstLineShowing;

        /** Number of lines showed by the text area * */
        protected int linesShowing;

        /** Variable to maintain the x offset of the cursor when moving up and down. If it's set to -1, the offset is reset * */
        float moveOffset;

        private float prefRows;

        /**
         * Allows to disable, enable disabling softwrapping. Note this isn't exposed property because TextArea can't handle it's by default.
         * You must have text area which can calculate its Max width such as {@link HighlightTextArea}
         */
        bool softwrap = true;
        float cursorX;

        public TextArea(string text, TextFieldStyle style) : base(text, style) { }

        public TextArea(string text, Skin skin, string stylename = "default") : base(text, skin, stylename)
        {
            writeEnters = true;
            cursorLine = 0;
            firstLineShowing = 0;
            moveOffset = -1;
            linesShowing = 0;
        }

        protected override int LetterUnderCursor(float x)
        {
            if (linesBreak.Count > 0)
            {
                if (cursorLine * 2 >= linesBreak.Count)
                {
                    return text.Length;
                }
                else
                {
                    float[] glyphPositions = this.glyphPositions.ToArray();
                    int start = linesBreak[cursorLine * 2];
                    x += glyphPositions[start];
                    int end = linesBreak[cursorLine * 2 + 1];
                    int i = start;
                    for (; i < end; i++)
                        if (glyphPositions[i] > x) break;
                    if (glyphPositions[i] - x <= x - glyphPositions[i - 1])
                    {
                        return Math.Min(i, text.Length);
                    }
                    return Math.Max(0, i - 1);
                }
            }
            else
            {
                return 0;
            }
        }

        /** Sets the preferred number of rows (lines) for this text area. Used to calculate preferred height */
        public void SetPrefRows(float prefRows)
        {
            this.prefRows = prefRows;
        }

        #region ILayout

        public override float PreferredHeight
        {
            get
            {
                if (prefRows <= 0)
                {
                    return base.PreferredHeight;
                }
                else
                {
                    float prefHeight = textHeight * prefRows;
                    if (style.background != null)
                    {
                        prefHeight = Math.Max(prefHeight + style.background.BottomHeight + style.background.TopHeight,
                                style.background.MinHeight);
                    }
                    return prefHeight;
                }
            }
        }

        #endregion

        /** Returns total number of lines that the text occupies * */
        public int GetLines()
        {
            return linesBreak.Count / 2 + (NewLineAtEnd() ? 1 : 0);
        }

        /** Returns if there's a new line at then end of the text * */
        public bool NewLineAtEnd()
        {
            return text.Length != 0 && (text[(text.Length - 1)] == ENTER_ANDROID || text[(text.Length - 1)] == ENTER_DESKTOP);
        }

        /** Moves the cursor to the given number line * */
        public void MoveCursorLine(int line)
        {
            if (line < 0)
            {
                cursorLine = 0;
                cursor = 0;
                moveOffset = -1;
            }
            else if (line >= GetLines())
            {
                int newLine = GetLines() - 1;
                cursor = text.Length;
                if (line > GetLines() || newLine == cursorLine)
                {
                    moveOffset = -1;
                }
                cursorLine = newLine;
            }
            else if (line != cursorLine)
            {
                if (moveOffset < 0)
                {
                    moveOffset = linesBreak.Count <= cursorLine * 2 ? 0
                            : glyphPositions[cursor] - glyphPositions[linesBreak[cursorLine * 2]];
                }
                cursorLine = line;
                cursor = cursorLine * 2 >= linesBreak.Count ? text.Length : linesBreak[cursorLine * 2];
                while (cursor < text.Length && cursor <= linesBreak[cursorLine * 2 + 1] - 1
                        && glyphPositions[cursor] - glyphPositions[linesBreak[cursorLine * 2]] < moveOffset)
                {
                    cursor++;
                }
                ShowCursor();
            }
        }
        
        /** Updates the current line, checking the cursor position in the text * */
        void UpdateCurrentLine()
        {
            int index = CalculateCurrentLineIndex(cursor);
            int line = index / 2;
            // Special case when cursor moves to the beginning of the line from the end of another and a word
            // wider than the box
            if (index % 2 == 0 || index + 1 >= linesBreak.Count || cursor != linesBreak[index]
                    || linesBreak[index + 1] != linesBreak[index])
            {
                if (line < linesBreak.Count / 2 || text.Length == 0 || text[text.Length - 1] == ENTER_ANDROID || text[text.Length - 1] == ENTER_DESKTOP)
                {
                    cursorLine = line;
                }
            }
        }

        /** Scroll the text area to show the line of the cursor * */
        void ShowCursor()
        {
            UpdateCurrentLine();
            if (cursorLine != firstLineShowing)
            {
                int step = cursorLine >= firstLineShowing ? 1 : -1;
                while (firstLineShowing > cursorLine || firstLineShowing + linesShowing - 1 < cursorLine)
                {
                    firstLineShowing += step;
                }
            }
        }

        /** Calculates the text area line for the given cursor position * */
        private int CalculateCurrentLineIndex(int cursor)
        {
            int index = 0;
            while (index < linesBreak.Count && cursor > linesBreak[index])
            {
                index++;
            }
            return index;
        }

        protected override void SizeChanged()
        {
            lastText = null; // Cause calculateOffsets to recalculate the line breaks.

            // The number of lines showed must be updated whenever the height is updated
            BitmapFont font = style.font;
            IDrawable background = style.background;
            float availableHeight = GetHeight() - (background == null ? 0 : background.BottomHeight + background.TopHeight);
            linesShowing = (int)Math.Floor(availableHeight / font.lineHeight);
        }

        protected override float GetTextY(BitmapFont font, IDrawable background)
        {
            float textY = 0;
            if (background != null)
            {
                textY = (int)(textY + background.TopHeight);
            }
            return textY;
        }

        protected override void DrawSelection(IDrawable selection, Graphics graphics, BitmapFont font, float x, float y, Color color)
        {
            int i = firstLineShowing * 2;
            float offsetY = 0;
            int minIndex = Math.Min(cursor, selectionStart);
            int maxIndex = Math.Max(cursor, selectionStart);
            while (i + 1 < linesBreak.Count && i < (firstLineShowing + linesShowing) * 2)
            {

                int lineStart = linesBreak[i];
                int lineEnd = linesBreak[i + 1];

                if (!((minIndex < lineStart && minIndex < lineEnd && maxIndex < lineStart && maxIndex < lineEnd)
                        || (minIndex > lineStart && minIndex > lineEnd && maxIndex > lineStart && maxIndex > lineEnd)))
                {

                    int start = Math.Max(linesBreak[i], minIndex);
                    int end = Math.Min(linesBreak[i + 1], maxIndex);

                    float selectionX = glyphPositions[start] - glyphPositions[linesBreak[i]];
                    float selectionWidth = glyphPositions[end] - glyphPositions[start];

                    selection.Draw(graphics, x + selectionX + fontOffset, y/* - textHeight*/ - font.descent / 2 + offsetY, selectionWidth, font.lineHeight, color);
                }

                offsetY += font.lineHeight;
                i += 2;
            }
        }

        protected override void DrawText(Graphics graphics, BitmapFont font, float x, float y, Color color)
        {
            float offsetY = 0;
            for (int i = firstLineShowing * 2; i < (firstLineShowing + linesShowing) * 2 && i < linesBreak.Count; i += 2)
            {
                font.Draw(graphics, displayText, x, y + offsetY, linesBreak[i], linesBreak[i + 1], color);
                offsetY += font.lineHeight;
            }
        }

        protected override void DrawCursor(IDrawable cursorPatch, Graphics graphics, BitmapFont font, float x, float y)
        {
            float textOffset = cursor >= glyphPositions.Count || cursorLine * 2 >= linesBreak.Count ? 0
                : glyphPositions[cursor] - glyphPositions[linesBreak[cursorLine * 2]];
            cursorX = textOffset + fontOffset - 1;//+ font.cursorX;
            cursorPatch.Draw(graphics, x + cursorX, y + font.descent / 2 + (cursorLine - firstLineShowing) * font.lineHeight, cursorPatch.MinWidth, font.lineHeight, color);
        }

        protected override void CalculateOffsets()
        {
            base.CalculateOffsets();
            if (!text.Equals(lastText))
            {
                lastText = text;
                var font = style.font;
                float maxWidthLine = GetWidth() - (style.background != null ? style.background.LeftWidth + style.background.RightWidth : 0);
                linesBreak.Clear();
                int lineStart = 0;
                int lastSpace = 0;
                char lastCharacter;
                for (int i = 0; i < text.Length; i++)
                {
                    lastCharacter = text[i];
                    if (lastCharacter == ENTER_DESKTOP || lastCharacter == ENTER_ANDROID)
                    {
                        linesBreak.Add(lineStart);
                        linesBreak.Add(i);
                        lineStart = i + 1;
                    }
                    else
                    {
                        lastSpace = (ContinueCursor(i, 0) ? lastSpace : i);
                        //layout.setText(font, text.subSequence(lineStart, i + 1));
                        if (font.MeasureString(text.Substring(lineStart, i + 1 - lineStart)).X > maxWidthLine && softwrap)
                        {
                            if (lineStart >= lastSpace)
                            {
                                lastSpace = i - 1;
                            }
                            linesBreak.Add(lineStart);
                            linesBreak.Add(lastSpace + 1);
                            lineStart = lastSpace + 1;
                            lastSpace = lineStart;
                        }
                    }
                }

                if (lineStart < text.Length)
                {
                    linesBreak.Add(lineStart);
                    linesBreak.Add(text.Length);
                }
                ShowCursor();
            }
        }


        protected override InputListener CreateListener()
        {
            return new TextAreaListener(this);
        }

        public override void SetSelection(int selectionStart, int selectionEnd)
        {
            base.SetSelection(selectionStart, selectionEnd);
            UpdateCurrentLine();
        }

        protected override void MoveCursor(bool forward, bool jump)
        {
            int count = forward ? 1 : -1;
            int index = (cursorLine * 2) + count;
            if (index >= 0 && index + 1 < linesBreak.Count && linesBreak[index] == cursor
                    && linesBreak[index + 1] == cursor)
            {
                cursorLine += count;
                if (jump)
                {
                    base.MoveCursor(forward, jump);
                }
                ShowCursor();
            }
            else
            {
                base.MoveCursor(forward, jump);
            }
            UpdateCurrentLine();
        }

        protected override bool ContinueCursor(int index, int offset)
        {
            int pos = CalculateCurrentLineIndex(index + offset);
            return base.ContinueCursor(index, offset) && (pos < 0 || pos >= linesBreak.Count - 2 || (linesBreak[pos + 1] != index)
                    || (linesBreak[pos + 1] == linesBreak[pos + 2]));
        }

        public int GetCursorLine()
        {
            return cursorLine;
        }

        public int GetFirstLineShowing()
        {
            return firstLineShowing;
        }

        public int GetLinesShowing()
        {
            return linesShowing;
        }

        public float GetCursorX()
        {
            return cursorX;
        }

        public float GetCursorY()
        {
            BitmapFont font = style.font;
            return -(-font.descent / 2 - (cursorLine - firstLineShowing + 1) * font.lineHeight);
        }

        #region Listener

        public class TextAreaListener : TextFieldClickListener
        {
            public TextAreaListener(TextArea t) : base(t)
            {

            }

            protected override void SetCursorPosition(float x, float y)
            {
                var t = (TextArea)this.t;

                t.moveOffset = -1;

                var background = t.style.background;
                BitmapFont font = t.style.font;

                float height = t.GetHeight();

                if (background != null)
                {
                    height -= background.TopHeight;
                    x -= background.LeftWidth;
                }
                x = Math.Max(0, x);
                if (background != null)
                {
                    y -= background.TopHeight;
                }

                //TODO fix scrolling
                y = SimpleMath.Clamp(y, 0, t.height - font.lineHeight);


                t.cursorLine = (int)Math.Floor((/*height - */y) / font.lineHeight) + t.firstLineShowing;
                t.cursorLine = Math.Max(0, Math.Min(t.cursorLine, t.GetLines() - 1));

                base.SetCursorPosition(x, y);
                t.UpdateCurrentLine();
            }

            public override bool KeyDown(InputEvent ev, int keycode)
            {
                var t = (TextArea)this.t;

                bool result = base.KeyDown(ev, keycode);
                Stage stage = t.GetStage();
                if (stage != null && stage.GetKeyboardFocus() == t) {
                    bool repeat = false;
                    bool shift = InputUtils.IsShiftDown();
                    if (keycode == (int)Keys.Down)
                    {
                        if (shift)
                        {
                            if (!t.hasSelection)
                            {
                                t.selectionStart = t.cursor;
                                t.hasSelection = true;
                            }
                        }
                        else
                        {
                            t.ClearSelection();
                        }
                        t.MoveCursorLine(t.cursorLine + 1);
                        repeat = true;

                    }
                    else if (keycode == (int)Keys.Up)
                    {
                        if (shift)
                        {
                            if (!t.hasSelection)
                            {
                                t.selectionStart = t.cursor;
                                t.hasSelection = true;
                            }
                        }
                        else
                        {
                            t.ClearSelection();
                        }
                        t.MoveCursorLine(t.cursorLine - 1);
                        repeat = true;

                    }
                    else
                    {
                        t.moveOffset = -1;
                    }
                    if (repeat)
                    {
                        ScheduleKeyRepeatTask(keycode);
                    }
                    t.ShowCursor();
                    return true;
                }
                return result;
            }

            public override bool KeyTyped(InputEvent ev, int keycode, char character)
            {
                var t = (TextArea)this.t;

                bool result = base.KeyTyped(ev, keycode, character);
                t.ShowCursor();
			    return result;
            }

            protected override void GoHome(bool jump)
            {
                var t = (TextArea)this.t;

                if (jump)
                {
                    t.cursor = 0;
                }
                else if (t.cursorLine * 2 < t.linesBreak.Count)
                {
                    t.cursor = t.linesBreak[t.cursorLine * 2];
                }
            }

            protected override void GoEnd(bool jump)
            {
                var t = (TextArea)this.t;

                if (jump || t.cursorLine >= t.GetLines())
                {
                    t.cursor = t.text.Length;
                }
                else if (t.cursorLine * 2 + 1 < t.linesBreak.Count)
                {
                    t.cursor = t.linesBreak[t.cursorLine * 2 + 1];
                }
            }
        }

        #endregion
    }
}
