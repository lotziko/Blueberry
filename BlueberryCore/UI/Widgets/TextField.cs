
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using static BlueberryCore.UI.ChangeListener;

namespace BlueberryCore.UI
{
    public class TextField : Element, IDisablable, IFocusable
    {
        private const char BACKSPACE = (char)8;
        protected const char ENTER_DESKTOP = '\r';
        protected const char ENTER_ANDROID = '\n';
        private const char TAB = '\t';
        private static readonly char DELETE = (char)127;
        private static readonly char BULLET = (char)8226;

        private static Vector2 tmp1 = new Vector2();
        private static Vector2 tmp2 = new Vector2();
        private static Vector2 tmp3 = new Vector2();

        public static float keyRepeatInitialTime = 0.4f;
        /** Repeat times for keys handled by {@link InputListener#keyDown(InputEvent, int)} such as navigation arrows */
        public static float keyRepeatTime = 0.04f;

        protected String text;
        protected int cursor, selectionStart;
        protected bool hasSelection;
        protected bool writeEnters;
        protected readonly List<float> glyphPositions = new List<float>();


        private string messageText;
        protected string displayText;
        StringBuilder textBuffer = new StringBuilder();
        InputListener inputListener;
        ITextFieldListener listener;
        ITextFieldFilter filter;

        bool focusTraversal = true, onlyFontChars = true, disabled;
        bool enterKeyFocusTraversal = false;
        private int textHAlign = AlignInternal.left;
        private float selectionX, selectionWidth;

        string undoText = "";
        int undoCursorPos = 0;
        long lastChangeTime;

        bool passwordMode;
        private StringBuilder passwordBuffer;
        private char passwordCharacter = BULLET;

        protected float fontOffset, textHeight, textOffset;
        float renderOffset;
        private int visibleTextStart, visibleTextEnd;
        private int maxLength = 0;

        private float blinkTime = 0.45f;
        bool cursorOn = true;
        long lastBlink;

        protected bool programmaticChangeEvents;

        protected TextFieldStyle style;
        private ClickListener clickListener;
        private bool drawBorder;
        private bool focusBorderEnabled = true;
        private bool inputValid = true;
        private bool ignoreEqualsTextChange = true;
        private bool readOnly = false;
        private float cursorPercentHeight = 0.8f;

        public Action<string> OnChange;

        public TextField(String text, Skin skin, string stylename = "default") : this(text, skin.Get<TextFieldStyle>(stylename)) { }

        public TextField(String text, TextFieldStyle style)
        {
            SetStyle(style);
            SetText(text);
            SetSize(PreferredWidth, PreferredHeight);
            AddListener(inputListener = CreateListener());
            AddListener(clickListener = new Listener(this));

            keyRepeatTask = new KeyRepeatTask(this);
        }

        protected virtual InputListener CreateListener()
        {
            return new TextFieldClickListener(this);
        }

        protected virtual int LetterUnderCursor(float x)
        {
            var halfSpaceSize = style.font.spaceWidth;
            x -= textOffset + fontOffset + halfSpaceSize /*- style.font.getData().cursorX*/ - glyphPositions[visibleTextStart];
            var n = glyphPositions.Count;
            for (var i = 0; i < n; i++)
            {
                if (glyphPositions[i] > x && i >= 1)
                {
                    if (glyphPositions[i] - x <= x - glyphPositions[i - 1])
                        return i;
                    return i - 1;
                }
            }
            return n - 1;
        }

        protected bool IsWordCharacter(char c)
        {
            return char.IsLetterOrDigit(c);
        }

        protected int[] WordUnderCursor(int at)
        {
            int start = at, right = text.Length, left = 0, index = start;
            for (; index < right; index++)
            {
                if (!IsWordCharacter(text[index]))
                {
                    right = index;
                    break;
                }
            }
            for (index = start - 1; index > -1; index--)
            {
                if (!IsWordCharacter(text[index]))
                {
                    left = index + 1;
                    break;
                }
            }
            return new int[] { left, right };
        }

        int[] WordUnderCursor(float x)
        {
            return WordUnderCursor(LetterUnderCursor(x));
        }

        bool WithinMaxLength(int size)
        {
            return maxLength <= 0 || size < maxLength;
        }

        public int GetMaxLength()
        {
            return maxLength;
        }

        public void SetMaxLength(int maxLength)
        {
            this.maxLength = maxLength;
        }

        /**
	     * When false, text set by {@link #setText(String)} may contain characters not in the font, a space will be displayed instead.
	     * When true (the default), characters not in the font are stripped by setText. Characters not in the font are always stripped
	     * when typed or pasted.
	     */
        public void SetOnlyFontChars(bool onlyFontChars)
        {
            this.onlyFontChars = onlyFontChars;
        }

        /**
	     * Returns the text field's style. Modifying the returned style may not have an effect until
	     * {@link #setStyle(VisTextFieldStyle)} is called.
	     */
        public TextFieldStyle GetStyle()
        {
            return style;
        }

        public void SetStyle(TextFieldStyle style)
        {
            this.style = style ?? throw new ArgumentNullException("style cannot be null.");
            textHeight = style.font.lineHeight;//textHeight = style.font.GetCapHeight() - style.font.getDescent() * 2;
            InvalidateHierarchy();
        }

        public override string ToString()
        {
            return GetText();
        }

        protected virtual void CalculateOffsets()
        {
            float visibleWidth = GetWidth();
            if (style.background != null)
                visibleWidth -= style.background.LeftWidth + style.background.RightWidth;

            int glyphCount = this.glyphPositions.Count;
            float[] glyphPositions = this.glyphPositions.ToArray();

            // Check if the cursor has gone out the left or right side of the visible area and adjust renderOffset.
            float distance = glyphPositions[Math.Max(0, cursor - 1)] + renderOffset;
            if (distance <= 0)
                renderOffset -= distance;
            else
            {
                int index = Math.Min(glyphCount - 1, cursor + 1);
                float minX = glyphPositions[index] - visibleWidth;
                if (-renderOffset < minX) renderOffset = -minX;
            }

            // Prevent renderOffset from starting too close to the end, eg after text was deleted.
            float maxOffset = 0;
            float width = glyphPositions[glyphCount - 1];
            for (int i = glyphCount - 2; i >= 0; i--)
            {
                float x = glyphPositions[i];
                if (width - x > visibleWidth) break;
                maxOffset = x;
            }
            if (-renderOffset > maxOffset) renderOffset = -maxOffset;

            // calculate first visible char based on render offset
            visibleTextStart = 0;
            float startX = 0;
            for (int i = 0; i < glyphCount; i++)
            {
                if (glyphPositions[i] >= -renderOffset)
                {
                    visibleTextStart = Math.Max(0, i);
                    startX = glyphPositions[i];
                    break;
                }
            }

            // calculate last visible char based on visible width and render offset
            int length = Math.Min(displayText.Length, glyphPositions.Length - 1);
            visibleTextEnd = Math.Min(length, cursor + 1);
            for (; visibleTextEnd <= length; visibleTextEnd++)
                if (glyphPositions[visibleTextEnd] > startX + visibleWidth) break;
            visibleTextEnd = Math.Max(0, visibleTextEnd - 1);

            if ((textHAlign & AlignInternal.left) == 0)
            {
                textOffset = visibleWidth - (glyphPositions[visibleTextEnd] - startX);
                if ((textHAlign & AlignInternal.center) != 0) textOffset = (float)Math.Round(textOffset * 0.5f);
            }
            else
                textOffset = startX + renderOffset;

            // calculate selection x position and width
            if (hasSelection)
            {
                int minIndex = Math.Min(cursor, selectionStart);
                int maxIndex = Math.Max(cursor, selectionStart);
                float minX = Math.Max(glyphPositions[minIndex] - glyphPositions[visibleTextStart], -textOffset);
                float maxX = Math.Min(glyphPositions[maxIndex] - glyphPositions[visibleTextStart], visibleWidth - textOffset);
                selectionX = minX;

                if (renderOffset == 0)
                    selectionX += textOffset;

                selectionWidth = maxX - minX;
            }
        }

        //Is needed becase render is being stopped when is not needed
        public override void Update(float delta)
        {
            base.Update(delta);
            var stage = GetStage();
            bool focused = (stage != null && stage.GetKeyboardFocus() == this);
            if (!focused) keyRepeatTask.Cancel();

            if (drawBorder && focused && !disabled)
            {
                Blink();
            }
        }

        #region Drawing

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            Stage stage = GetStage();
            bool focused = (stage != null && stage.GetKeyboardFocus() == this);

            BitmapFont font = style.font;
            Color fontColor = (disabled && style.disabledFontColor != null) ? style.disabledFontColor
                    : ((focused && style.focusedFontColor != null) ? style.focusedFontColor : style.fontColor);
            IDrawable selection = style.selection;
            IDrawable cursorPatch = style.cursor;
            IDrawable background = (disabled && style.disabledBackground != null) ? style.disabledBackground
                    : ((focused && style.focusedBackground != null) ? style.focusedBackground : style.background);

            if (!disabled && style.backgroundOver != null && (clickListener.IsOver() || focused))
            {
                background = style.backgroundOver;
            }

            var color = new Color(this.color.R, this.color.G, this.color.B, (int)(this.color.A * parentAlpha));
            float x = GetX();
            float y = GetY();
            float width = GetWidth();
            float height = GetHeight();

            float bgLeftWidth = 0, bgRightWidth = 0;
            if (background != null)
            {
                background.Draw(graphics, x, y, width, height, color);
                bgLeftWidth = background.LeftWidth;
                bgRightWidth = background.RightWidth;
            }
            graphics.Flush();

            float textY = GetTextY(font, background);
            var yOffset = (textY < 0) ? -textY - font.lineHeight / 2f + GetHeight() / 2 : 0;
            CalculateOffsets();

            if (focused && hasSelection && selection != null)
            {
                DrawSelection(selection, graphics, font, x + bgLeftWidth, y + textY, color);
            }
            
            if (displayText.Length == 0)
            {
                if (!focused && messageText != null)
                {
                    Color messageFontColor;
                    if (style.messageFontColor != null)
                    {
                        messageFontColor = new Color(style.messageFontColor.R, style.messageFontColor.G, style.messageFontColor.B, (style.messageFontColor.A * color.A) / 255 * parentAlpha);
                    }
                    else
                        messageFontColor = new Color(180, 180, 180, color.A * parentAlpha);
                    BitmapFont messageFont = style.messageFont ?? font;
                    messageFont.Draw(graphics, messageText, x + bgLeftWidth, y + textY + yOffset, 0, messageText.Length, messageFontColor);
                }
            }
            else
            {
                DrawText(graphics, font, x + bgLeftWidth, y + textY + yOffset, new Color(fontColor.R, fontColor.G, fontColor.B, (int)(fontColor.A * parentAlpha)));
            }
            if (drawBorder && focused && !disabled)
            {
                if (cursorOn && cursorPatch != null)
                {
                    DrawCursor(cursorPatch, graphics, font, x + bgLeftWidth, y + textY);
                }
            }

            if (focusBorderEnabled && drawBorder && style.focusedBorder != null)
            {
                style.focusedBorder.Draw(graphics, GetX(), GetY(), GetWidth(), GetHeight(), color);
            }
        }

        protected virtual void DrawText(Graphics graphics, BitmapFont font, float x, float y, Color color)
        {
            font.Draw(graphics, displayText, x, y, visibleTextStart, visibleTextEnd, color);
        }

        protected virtual float GetTextY(BitmapFont font, IDrawable background)
        {
            float height = GetHeight();
            float textY = textHeight / 2 + font.descent;
            if (background != null)
            {
                var bottom = background.BottomHeight;
                textY = textY - (height - background.TopHeight - bottom) / 2 + bottom;
            }
            else
            {
                textY = textY - height / 2;
            }

            return textY;
        }
        
        protected virtual void DrawSelection(IDrawable selection, Graphics graphics, BitmapFont font, float x, float y, Color color)
        {
            selection.Draw(graphics, x + selectionX + textOffset + fontOffset, y - font.descent / 2, selectionWidth, textHeight, color);
        }
        
        protected virtual void DrawCursor(IDrawable cursorPatch, Graphics graphics, BitmapFont font, float x, float y)
        {
            cursorPatch.Draw(graphics,
                x + textOffset + glyphPositions[cursor] - glyphPositions[visibleTextStart] + fontOffset - 1 /*font.getData().cursorX*/,
                y - font.descent / 2, cursorPatch.MinWidth, textHeight, color);
        }

        #endregion

        void UpdateDisplayText()
        {
            var textLength = text.Length;

            textBuffer.Clear();
            for (var i = 0; i < textLength; i++)
            {
                var c = text[i];
                textBuffer.Append(style.font.HasCharacter(c) ? c : ' ');
            }
            var newDisplayText = textBuffer.ToString();

            if (passwordMode && style.font.HasCharacter(passwordCharacter))
            {
                if (passwordBuffer == null)
                    passwordBuffer = new StringBuilder(newDisplayText.Length);
                else if (passwordBuffer.Length > textLength)
                    passwordBuffer.Clear();

                for (var i = passwordBuffer.Length; i < textLength; i++)
                    passwordBuffer.Append(passwordCharacter);
                displayText = passwordBuffer.ToString();
            }
            else
            {
                displayText = newDisplayText;
            }

            //layout.setText( font, displayText );
            glyphPositions.Clear();
            float x = 0;
            if (displayText.Length > 0)
            {
                for (var i = 0; i < displayText.Length; i++)
                {
                    var region = style.font.GetGlyph(displayText[i]);
                    // we dont have fontOffset in BitmapFont, it is the first Glyph in a GlyphRun
                    //if( i == 0 )
                    //	fontOffset = region.xAdvance;
                    glyphPositions.Add(x);
                    x += region._xadvance;
                }
                //GlyphRun run = layout.runs.first();
                //FloatArray xAdvances = run.xAdvances;
                //fontOffset = xAdvances.first();
                //for( int i = 1, n = xAdvances.size; i < n; i++ )
                //{
                //	glyphPositions.add( x );
                //	x += xAdvances.get( i );
                //}
            }
            else
            {
                fontOffset = 0;
            }
            glyphPositions.Add(x);

            if (selectionStart > newDisplayText.Length)
                selectionStart = textLength;
            if (GetStage()?.GetKeyboardFocus() == this)
                OnChange?.Invoke(text);
        }

        private void Blink()
        {
            /*if (!Gdx.graphics.isContinuousRendering())
            {
                cursorOn = true;
                return;
            }*/
            long time = TimeUtils.CurrentTimeMillis();
            if (time - lastBlink/*(time - lastBlink) / 1000000000.0f*/ > blinkTime * 1000)
            {
                cursorOn = !cursorOn;
                lastBlink = time;
                Core.RequestRender();
            }
        }
        
        #region Clipboard

        /** Copies the contents of this TextField to the {@link Clipboard} implementation set on this TextField. */
        public void Copy()
        {
            if (hasSelection && !passwordMode)
            {
                int beginIndex = Math.Min(cursor, selectionStart);
                int endIndex = Math.Max(cursor, selectionStart) - beginIndex;
                Clipboard.SetText(text.Substring(Math.Max(0, beginIndex), Math.Min(text.Length, endIndex)));
            }
        }

        /**
	     * Copies the selected contents of this TextField to the {@link Clipboard} implementation set on this TextField, then removes
	     * it.
	     */
        public void Cut()
        {
            Cut(programmaticChangeEvents);
        }

        void Cut(bool fireChangeEvent)
        {
            if (hasSelection && !passwordMode)
            {
                Copy();
                cursor = Delete(fireChangeEvent);
                UpdateDisplayText();
            }
        }

        void Paste(string content, bool fireChangeEvent)
        {
            if (content == null) return;
            StringBuilder buffer = new StringBuilder();
            int textLength = text.Length;
            if (hasSelection) textLength -= Math.Abs(cursor - selectionStart);
            for (int i = 0, n = content.Length; i < n; i++)
            {
                if (!WithinMaxLength(textLength + buffer.Length)) break;
                char c = content[i];
                if (!(writeEnters && (c == ENTER_ANDROID || c == ENTER_DESKTOP)))
                {
                    if (c == '\r' || c == '\n') continue;
                    if (onlyFontChars && !style.font.HasCharacter(c)) continue;
                    if (filter != null && !filter.AcceptChar(this, c)) continue;
                }
                buffer.Append(c);
            }
            content = buffer.ToString();

            if (hasSelection) cursor = Delete(fireChangeEvent);
            if (fireChangeEvent)
                ChangeText(text, Insert(cursor, content, text));
            else
                text = Insert(cursor, content, text);
            UpdateDisplayText();
            cursor += content.Length;
        }

        string Insert(int position, string text, string to)
        {
            if (to.Length == 0) return text;
            return to.Substring(0, position) + text + to.Substring(position, to.Length - position);
        }

        int Delete(bool fireChangeEvent)
        {
            var from = selectionStart;
            var to = cursor;
            var minIndex = Math.Min(from, to);
            var maxIndex = Math.Max(from, to);
            var newText = (minIndex > 0 ? text.Substring(0, minIndex) : "")
                          + (maxIndex < text.Length ? text.Substring(maxIndex, text.Length - maxIndex) : "");

            if (fireChangeEvent)
                ChangeText(text, newText);
            else
                text = newText;

            ClearSelection();
            return minIndex;
        }

        /**
	     * Focuses the next TextField. If none is found, the keyboard is hidden. Does nothing if the text field is not in a stage.
	     * @param up If true, the TextField with the same or next smallest y coordinate is found, else the next highest.
	     */
        public void Next(bool up)
        {
            var stage = GetStage();
            if (stage == null) return;
            tmp1.Set(GetX(), GetY());
            tmp1 = GetParent().LocalToStageCoordinates(tmp1);
            var textField = FindNextTextField(stage.GetElements(), null, tmp2, tmp1, up);
            if (textField == null)
            { // Try to wrap around.
                if (up)
                    tmp1.Set(float.MaxValue, float.MaxValue);
                else
                    tmp1.Set(float.MinValue, float.MinValue);
                textField = FindNextTextField(GetStage().GetElements(), null, tmp2, tmp1, up);
            }
            if (textField != null)
            {
                textField.FocusField();
                textField.SetCursorPosition(textField.GetText().Length);
            }
            else
            {
                //Gdx.input.setOnscreenKeyboardVisible(false);
            }
        }

        private TextField FindNextTextField(List<Element> elements, TextField best, Vector2 bestCoords, Vector2 currentCoords, bool up)
        {
            var modalWindow = FindModalWindow(this);

            for (int i = 0, n = elements.Count; i < n; i++)
            {
                var element = elements[i];
                if (element == this) continue;
                if (element is TextField) {
                var textField = element as TextField;

                if (modalWindow != null)
                {
                    var nextFieldModalWindow = FindModalWindow(textField);
                    if (nextFieldModalWindow != modalWindow) continue;
                }

                if (textField.disabled || !textField.focusTraversal || IsActorVisibleInStage(textField) == false)
                    continue;

                tmp3.Set(element.GetX(), element.GetY());
                Vector2 actorCoords = element.GetParent().LocalToStageCoordinates(tmp3);
                if ((actorCoords.Y > currentCoords.Y || (actorCoords.Y == currentCoords.Y && actorCoords.X > currentCoords.X)) ^ up)
                {
                    if (best == null
                            || (actorCoords.Y < bestCoords.Y || (actorCoords.Y == bestCoords.Y && actorCoords.X < bestCoords.X)) ^ up)
                    {
                        best = element as TextField;
                        bestCoords.Set(actorCoords);
                    }
                }
            } else if (element is Group)
				best = FindNextTextField(((Group)element).GetElements(), best, bestCoords, currentCoords, up);
        }
		return best;
	}

        #endregion

        #region Getters and setters

        /**
	     * Checks if actor is visible in stage acknowledging parent visibility.
	     * If any parent returns false from isVisible then this method return false.
	     * True is returned when this actor and all its parent are visible.
	     */
        private bool IsActorVisibleInStage(Element element)
        {
            if (element == null) return true;
            if (element.IsVisible() == false) return false;
            return IsActorVisibleInStage(element.GetParent());
        }

        private Window FindModalWindow(Element element)
        {
            if (element == null) return null;
            if (element is Window && ((Window)element).IsModal()) return (Window)element;
            return FindModalWindow(element.GetParent());
        }

        public InputListener GetDefaultInputListener()
        {
            return inputListener;
        }

        /** @param listener May be null. */
        public void SetTextFieldListener(ITextFieldListener listener)
        {
            this.listener = listener;
        }

        /** @param filter May be null. */
        public void SetTextFieldFilter(ITextFieldFilter filter)
        {
            this.filter = filter;
        }

        public ITextFieldFilter GetTextFieldFilter()
        {
            return filter;
        }

        /** If true (the default), tab/shift+tab will move to the next text field. */
        public void SetFocusTraversal(bool focusTraversal)
        {
            this.focusTraversal = focusTraversal;
        }

        /**
         * If true, enter will move to the next text field with has focus traversal enabled.
         * False by default. Note that to enable or disable focus traversal completely you must
         * use {@link #setFocusTraversal(bool)}
         */
        public void SetEnterKeyFocusTraversal(bool enterKeyFocusTraversal)
        {
            this.enterKeyFocusTraversal = enterKeyFocusTraversal;
        }

        /** @return May be null. */
        public String GetMessageText()
        {
            return messageText;
        }

        /**
         * Sets the text that will be drawn in the text field if no text has been entered.
         * @param messageText may be null.
         */
        public void SetMessageText(string messageText)
        {
            this.messageText = messageText;
        }

        /** @param str If null, "" is used. */
        public void AppendText(string str)
        {
            if (str == null) str = "";

            ClearSelection();
            cursor = text.Length;
            Paste(str, programmaticChangeEvents);
        }

        /** @param str If null, "" is used. */
        public virtual void SetText(string str/*, bool triggerEvent = true*/)
        {
            if (str == null) str = "";
            if (ignoreEqualsTextChange && str.Equals(text)) return;

            ClearSelection();
            String oldText = text;
            text = "";
            Paste(str, false);
            if (programmaticChangeEvents) ChangeText(oldText, text);
            cursor = 0;

            //if (triggerEvent)
                //OnChange?.Invoke(text);
        }

        /** @return Never null, might be an empty string. */
        public string GetText()
        {
            return text;
        }

        /**
         * @param oldText May be null.
         * @return True if the text was changed.
         */
        protected virtual bool ChangeText(string oldText, string newText)
        {
            if (ignoreEqualsTextChange && newText.Equals(oldText)) return false;
            text = newText;
            BeforeChangeEventFired();
            var changeEvent = Pool<ChangeEvent>.Obtain();
		    bool cancelled = Fire(changeEvent);
            text = cancelled? oldText : newText;
            Pool<ChangeEvent>.Free(changeEvent);
            return !cancelled;
        }

        void BeforeChangeEventFired()
        {

        }

        public bool GetProgrammaticChangeEvents()
        {
            return programmaticChangeEvents;
        }

        /**
         * If false, methods that change the text will not fire {@link ChangeEvent}, the event will be fired only when user changes
         * the text.
         */
        public void SetProgrammaticChangeEvents(bool programmaticChangeEvents)
        {
            this.programmaticChangeEvents = programmaticChangeEvents;
        }

        public int GetSelectionStart()
        {
            return selectionStart;
        }

        public string GetSelection()
        {
            return hasSelection ? text.Substring(Math.Min(selectionStart, cursor), Math.Max(selectionStart, cursor)) : "";
        }

        public bool IsTextSelected()
        {
            return hasSelection;
        }

        /** Sets the selected text. */
        public virtual void SetSelection(int selectionStart, int selectionEnd)
        {
            if (selectionStart < 0) throw new ArgumentOutOfRangeException("selectionStart must be >= 0");
            if (selectionEnd < 0) throw new ArgumentOutOfRangeException("selectionEnd must be >= 0");
            selectionStart = Math.Min(text.Length, selectionStart);
            selectionEnd = Math.Min(text.Length, selectionEnd);
            if (selectionEnd == selectionStart)
            {
                ClearSelection();
                return;
            }
            if (selectionEnd < selectionStart)
            {
                int temp = selectionEnd;
                selectionEnd = selectionStart;
                selectionStart = temp;
            }

            hasSelection = true;
            this.selectionStart = selectionStart;
            cursor = selectionEnd;
        }

        public void SelectAll()
        {
            SetSelection(0, text.Length);
        }

        public void ClearSelection()
        {
            hasSelection = false;
        }

        /** Clears VisTextField text. If programmatic change events are disabled then this will not fire change event. */
        public void ClearText()
        {
            SetText("");
        }

        /** Sets the cursor position and clears any selection. */
        public void SetCursorPosition(int cursorPosition)
        {
            if (cursorPosition < 0) throw new ArgumentOutOfRangeException("cursorPosition must be >= 0");
            ClearSelection();
            cursor = Math.Min(cursorPosition, text.Length);
        }

        public int GetCursorPosition()
        {
            return cursor;
        }

        public void SetCursorAtTextEnd()
        {
            SetCursorPosition(0);
            CalculateOffsets();
            SetCursorPosition(GetText().Length);
        }

        /** @param cursorPercentHeight cursor size, value from 0..1 range */
        public void SetCursorPercentHeight(float cursorPercentHeight)
        {
            if (cursorPercentHeight < 0 || cursorPercentHeight > 1)
                throw new ArgumentOutOfRangeException("cursorPercentHeight must be >= 0 and <= 1");
            this.cursorPercentHeight = cursorPercentHeight;
        }

        /** Default is an instance of {@link DefaultOnscreenKeyboard}. */
        /*public OnscreenKeyboard getOnscreenKeyboard()
        {
            return keyboard;
        }

        public void setOnscreenKeyboard(OnscreenKeyboard keyboard)
        {
            this.keyboard = keyboard;
        }

        public void setClipboard(Clipboard clipboard)
        {
            this.clipboard = clipboard;
        }*/

        /**
         * Sets text horizontal alignment (left, center or right).
         * @see Align
         */
        public void SetAlignment(int alignment)
        {
            this.textHAlign = alignment;
        }

        /**
         * If true, the text in this text field will be shown as bullet characters.
         * @see #setPasswordCharacter(char)
         */
        public void SetPasswordMode(bool passwordMode)
        {
            this.passwordMode = passwordMode;
            UpdateDisplayText();
        }

        public bool IsPasswordMode()
        {
            return passwordMode;
        }

        /**
         * Sets the password character for the text field. The character must be present in the {@link BitmapFont}. Default is 149
         * (bullet).
         */
        public void SetPasswordCharacter(char passwordCharacter)
        {
            this.passwordCharacter = passwordCharacter;
            if (passwordMode) UpdateDisplayText();
        }

        public void SetBlinkTime(float blinkTime)
        {
            this.blinkTime = blinkTime;
        }

        public bool IsDisabled()
        {
            return disabled;
        }

       
        public void SetDisabled(bool disabled)
        {
            this.disabled = disabled;
            if (disabled)
            {
                FocusManager.ResetFocus(GetStage(), this);
                keyRepeatTask.Cancel();
            }
        }

        public bool IsReadOnly()
        {
            return readOnly;
        }

        public void SetReadOnly(bool readOnly)
        {
            this.readOnly = readOnly;
        }

        protected virtual void MoveCursor(bool forward, bool jump)
        {
            int limit = forward ? text.Length : 0;
            int charOffset = forward ? 0 : -1;
            while ((forward ? ++cursor < limit : --cursor > limit) && jump)
            {
                if (!ContinueCursor(cursor, charOffset)) break;
            }
        }

        protected virtual bool ContinueCursor(int index, int offset)
        {
            char c = text[index + offset];
            return IsWordCharacter(c);
        }

        /** Focuses this field, field must be added to stage before this method can be called */
        public void FocusField()
        {
            if (disabled) return;
            Stage stage = GetStage();
            FocusManager.SwitchFocus(stage, this);
            SetCursorPosition(0);
            selectionStart = 0;
            //make sure textOffset was updated, prevent issue when there was long text selected and it was changed to short text
            //and field was focused. Without it textOffset would stay at max value and only one last letter will be visible in field
            CalculateOffsets();
            if (stage != null) stage.SetKeyboardFocus(this);
            //keyboard.show(true);
            hasSelection = true;
        }

        public void FocusLost()
        {
            drawBorder = false;
        }

        public void FocusGained()
        {
            drawBorder = true;
        }

        public bool IsEmpty()
        {
            return text.Length == 0;
        }

        public bool IsInputValid()
        {
            return inputValid;
        }

        public void SetInputValid(bool inputValid)
        {
            this.inputValid = inputValid;
        }

        
        public bool IsFocusBorderEnabled()
        {
            return focusBorderEnabled;
        }
        
        public void SetFocusBorderEnabled(bool focusBorderEnabled)
        {
            this.focusBorderEnabled = focusBorderEnabled;
        }

        /** @see #setIgnoreEqualsTextChange(bool) */
        public bool IsIgnoreEqualsTextChange()
        {
            return ignoreEqualsTextChange;
        }

        /**
         * Allows to control whether change event is sent when text field's text is changed to same same as was it before.
         * Eg. current text field is 'abc' and {@link #setText(String)} is called it with 'abc' again.
         * @param ignoreEqualsTextChange if true then setting text to the same as it was before will NOT fire change event.
         * Default is true however it is false default {@link VisValidatableTextField} to prevent form refreshment issues -
         * see issue VisEditor#165
         */
        public void SetIgnoreEqualsTextChange(bool ignoreEqualsTextChange)
        {
            this.ignoreEqualsTextChange = ignoreEqualsTextChange;
        }

        #endregion

        #region ILayout

        public override float PreferredWidth
        {
            get
            {
                return 150;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                float prefHeight = textHeight;
                if (style.background != null)
                {
                    prefHeight = Math.Max(prefHeight + style.background.BottomHeight + style.background.TopHeight,
                            style.background.MinHeight);
                }
                return prefHeight;
            }
        }

        #endregion

        #region Listeners

        /**
	     * Interface for listening to typed characters.
	     */
        public interface ITextFieldListener
        {
            void KeyTyped(TextField textField, char c);
        }

        public class TextFieldClickListener : ClickListener
        {
            protected readonly TextField t;

            public TextFieldClickListener(TextField t)
            {
                this.t = t;
            }

            public override void Clicked(InputEvent ev, float x, float y)
            {
                int count = GetTapCount() % 4;
                if (count == 0) t.ClearSelection();
                if (count == 2)
                {
                    int[] array = t.WordUnderCursor(x);
                    t.SetSelection(array[0], array[1]);
                }
                if (count == 3) t.SelectAll();
            }

            public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (!base.TouchDown(ev, x, y, pointer, button)) return false;
                if (pointer == 0 && button != 0) return false;
                if (t.disabled) return true;
                var stage = t.GetStage();
                FocusManager.SwitchFocus(stage, t);
                SetCursorPosition(x, y);
                t.selectionStart = t.cursor;
                if (stage != null) stage.SetKeyboardFocus(t);
                //if (readOnly == false) keyboard.show(true);
                t.hasSelection = true;
                return true;
            }

            public override void TouchDragged(InputEvent ev, float x, float y, int pointer)
            {
                base.TouchDragged(ev, x, y, pointer);
                SetCursorPosition(x, y);
            }

            public override void TouchUp(InputEvent ev, float x, float y, int pointer, int button)
            {
                if (t.selectionStart == t.cursor) t.hasSelection = false;
                base.TouchUp(ev, x, y, pointer, button);
            }

            protected virtual void SetCursorPosition(float x, float y)
            {
                t.lastBlink = 0;
                t.cursorOn = false;
                t.cursor = Math.Min(t.LetterUnderCursor(x), t.text.Length);
            }

            protected virtual void GoHome(bool jump)
            {
                t.cursor = 0;
            }

            protected virtual void GoEnd(bool jump)
            {
                t.cursor = t.text.Length;
            }

            public override bool KeyDown(InputEvent ev, int keycode)
            {
                if (t.disabled) return false;

                t.lastBlink = 0;
                t.cursorOn = false;

                var stage = t.GetStage();
                if (stage == null || stage.GetKeyboardFocus() != t) return false;
                if (t.drawBorder == false) return false;

                bool repeat = false;
                bool ctrl = InputUtils.IsCtrlDown();
                bool jump = ctrl && !t.passwordMode;

                if (ctrl)
                {
                    if (keycode == (int)Keys.V && t.readOnly == false)
                    {
                        t.Paste(Clipboard.GetText(), true);
                        repeat = true;
                    }
                    if (keycode == (int)Keys.C || keycode == (int)Keys.Insert)
                    {
                        t.Copy();
                        return true;
                    }
                    if (keycode == (int)Keys.X && t.readOnly == false)
                    {
                        t.Cut(true);
                        return true;
                    }
                    if (keycode == (int)Keys.A)
                    {
                        t.SelectAll();
                        return true;
                    }
                    if (keycode == (int)Keys.Z && t.readOnly == false)
                    {
                        var oldText = t.text;
                        int oldCursorPos = t.GetCursorPosition();
                        t.SetText(t.undoText);
                        t.SetCursorPosition(SimpleMath.Clamp(t.cursor, 0, t.undoText.Length));
                        t.undoText = oldText;
                        t.undoCursorPos = oldCursorPos;
                        t.UpdateDisplayText();
                        return true;
                    }
                }

                if (InputUtils.IsShiftDown())
                {
                    if (keycode == (int)Keys.Insert && t.readOnly == false) t.Paste(Clipboard.GetText(), true);
                    if (keycode == (int)Keys.Delete && t.readOnly == false) t.Cut(true);
                    //selection:
                    {
                        int temp = t.cursor;
                        keys:
                        {
                            if (keycode == (int)Keys.Left)
                            {
                                t.MoveCursor(false, jump);
                                repeat = true;
                                goto keys;
                            }
                            if (keycode == (int)Keys.Right)
                            {
                                t.MoveCursor(true, jump);
                                repeat = true;
                                goto keys;
                            }
                            if (keycode == (int)Keys.Home)
                            {
                                GoHome(jump);
                                goto keys;
                            }
                            if (keycode == (int)Keys.End)
                            {
                                GoEnd(jump);
                                goto keys;
                            }
                            //goto selection;
                        }
                        if (!t.hasSelection)
                        {
                            t.selectionStart = temp;
                            t.hasSelection = true;
                        }
                    }
                }
                else
                {
                    // Cursor movement or other keys (kills selection).
                    if (keycode == (int)Keys.Left)
                    {
                        t.MoveCursor(false, jump);
                        t.ClearSelection();
                        repeat = true;
                    }
                    if (keycode == (int)Keys.Right)
                    {
                        t.MoveCursor(true, jump);
                        t.ClearSelection();
                        repeat = true;
                    }
                    if (keycode == (int)Keys.Home)
                    {
                        GoHome(jump);
                        t.ClearSelection();
                    }
                    if (keycode == (int)Keys.End)
                    {
                        GoEnd(jump);
                        t.ClearSelection();
                    }
                }
                t.cursor = SimpleMath.Clamp(t.cursor, 0, t.text.Length);

                if (repeat)
                {
                    ScheduleKeyRepeatTask(keycode);
                }
                return true;
            }

            protected void ScheduleKeyRepeatTask(int keycode)
            {
                if (!t.keyRepeatTask.IsScheduled() || t.keyRepeatTask.keycode != keycode)
                {
                    t.keyRepeatTask.keycode = keycode;
                    t.keyRepeatTask.Cancel();
                    if (Input.IsKeyPressed((Keys)t.keyRepeatTask.keycode))
                    { //issue #179
                        Timer.Schedule(t.keyRepeatTask, TimeSpan.FromSeconds(keyRepeatInitialTime), TimeSpan.FromSeconds(keyRepeatTime));
                    }
                }
            }

            public override bool KeyUp(InputEvent ev, int keycode)
            {
                if (t.disabled) return false;
                t.keyRepeatTask.Cancel();
                return true;
            }

            public override bool KeyTyped(InputEvent ev, int keycode, char character)
            {
                if (t.disabled || t.readOnly) return false;

                // Disallow "typing" most ASCII control characters, which would show up as a space when onlyFontChars is true.
                switch (character)
                {
                    case BACKSPACE:
                    case TAB:
                    case ENTER_ANDROID:
                    case ENTER_DESKTOP:
                        break;
                    default:
                        if (character < 32) return false;
                        break;
                }

                //lotziko fix for selection lose when typing wrong char
                if (t.filter != null && !t.filter.AcceptChar(t, character) && !char.IsControl(character))
                    return false;

                var stage = t.GetStage();
                if (stage == null || stage.GetKeyboardFocus() != t) return false;

                //if (UIUtils.isMac && Gdx.input.isKeyPressed(Keys.SYM)) return true;

                if (t.focusTraversal && (character == TAB || (character == ENTER_ANDROID && t.enterKeyFocusTraversal)))
                {
                    t.Next(InputUtils.IsShiftDown());
                }
                else
                {
                    bool delete = character == DELETE;
                    bool backspace = character == BACKSPACE;
                    bool enter = character == ENTER_DESKTOP || character == ENTER_ANDROID;
                    bool add = enter ? t.writeEnters : (!t.onlyFontChars || t.style.font.HasCharacter(character));
                    bool remove = backspace || delete;
                    if (add || remove)
                    {
                        string oldText = t.text;
                        int oldCursor = t.cursor;
                        if (t.hasSelection)
                            t.cursor = t.Delete(false);
                        else
                        {
                            if (backspace && t.cursor > 0)
                            {
                                t.text = t.text.Substring(0, t.cursor - 1) + t.text.Substring(t.cursor--);
                                t.renderOffset = 0;
                            }
                            if (delete && t.cursor < t.text.Length)
                            {
                                t.text = t.text.Substring(0, t.cursor) + t.text.Substring(t.cursor + 1);
                            }
                        }
                        if (add && !remove)
                        {
                            // Character may be added to the text.
                            if (!enter && t.filter != null && !t.filter.AcceptChar(t, character)) return true;
                            if (!t.WithinMaxLength(t.text.Length)) return true;
                            string insertion = enter ? "\n" : character + "";
                            t.text = t.Insert(t.cursor++, insertion, t.text);
                        }
                        if (t.ChangeText(oldText, t.text))
                        {
                            long time = TimeUtils.CurrentTimeMillis();
                            if (time - 750 > t.lastChangeTime)
                            {
                                t.undoText = oldText;
                                t.undoCursorPos = t.GetCursorPosition() - 1;
                            }
                            t.lastChangeTime = time;
                        }
                        else
                            t.cursor = oldCursor;
                        t.UpdateDisplayText();
                    }
                }
                if (t.listener != null) t.listener.KeyTyped(t, character);
                return true;
            }

        }
        
        private class Listener : ClickListener
        {
            private TextField t;

            public Listener(TextField t)
            {
                this.t = t;
            }

            public override void Enter(InputEvent ev, float x, float y, int pointer, Element fromElement)
            {
                base.Enter(ev, x, y, pointer, fromElement);
                if (pointer == -1 && t.IsDisabled() == false)
                {
                    Mouse.SetCursor(MouseCursor.IBeam);
                }
            }

            public override void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
            {
                base.Exit(ev, x, y, pointer, toElement);
                if (pointer == -1)
                {
                    Mouse.SetCursor(MouseCursor.Arrow);
                }
            }
        }

        #endregion

        #region Filters

        /**
        * Interface for filtering characters entered into the text field.
        */
        public interface ITextFieldFilter
        {
            bool AcceptChar(TextField textField, char c);
        }

        public class DigitsOnlyFilter : ITextFieldFilter
        {
            public bool AcceptChar(TextField textField, char c)
            {
                return char.IsDigit(c);
            }
        }

        #endregion

        #region Task

        private readonly KeyRepeatTask keyRepeatTask;

        private class KeyRepeatTask : Task
        {
            private readonly TextField tf;
            public int keycode;

            public KeyRepeatTask(TextField tf)
            {
                this.tf = tf;
            }

            public override void Run()
            {
                tf.inputListener.KeyDown(null, keycode);
            }
        }

        #endregion

    }

    public class TextFieldStyle
    {
        public BitmapFont font;
        public Color fontColor;
        /** Optional. */
        public Color focusedFontColor, disabledFontColor;
        /** Optional. */
        public IDrawable background, focusedBackground, disabledBackground, cursor, selection;
        /** Optional. */
        public BitmapFont messageFont;
        /** Optional. */
        public Color messageFontColor;

        public IDrawable focusedBorder;
        public IDrawable backgroundOver;

        public TextFieldStyle()
        {
        }

        public TextFieldStyle(BitmapFont font, Color fontColor, IDrawable cursor, IDrawable selection, IDrawable background)
        {
            this.background = background;
            this.cursor = cursor;
            this.font = font;
            this.fontColor = fontColor;
            this.selection = selection;
        }

        public TextFieldStyle(TextFieldStyle style)
        {
            this.messageFont = style.messageFont;
            if (style.messageFontColor != null) this.messageFontColor = new Color(style.messageFontColor, style.messageFontColor.A);
            this.background = style.background;
            this.focusedBackground = style.focusedBackground;
            this.disabledBackground = style.disabledBackground;
            this.cursor = style.cursor;
            this.font = style.font;
            if (style.fontColor != null) this.fontColor = new Color(style.fontColor, style.fontColor.A);
            if (style.focusedFontColor != null) this.focusedFontColor = new Color(style.focusedFontColor, style.focusedFontColor.A);
            if (style.disabledFontColor != null) this.disabledFontColor = new Color(style.disabledFontColor, style.disabledFontColor.A);
            this.selection = style.selection;
        }
    }
}
