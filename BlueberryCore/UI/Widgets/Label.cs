using Microsoft.Xna.Framework;

namespace BlueberryCore.UI
{
    public class Label : Element
    {
        LabelStyle _style;
        string _text;
        float _fontScaleX = 1;
        float _fontScaleY = 1;
        int labelAlign = AlignInternal.center;
        /// <summary>
        /// String is "..." when do not fit
        /// </summary>
        string _ellipsis;
        bool _wrapText;

        // internal state
        string _wrappedString;
        bool _prefSizeInvalid;
        float _lastPrefHeight;
        Vector2 _prefSize;
        Vector2 _textPosition;

        public Label(string text, BitmapFont font, Color fontColor) : this(text, new LabelStyle(font, fontColor)) { }

        public Label(string text, Skin skin, string stylename = "default") : this(text, skin.Get<LabelStyle>(stylename)) { }

        public Label(string text, LabelStyle style)
        {
            SetStyle(style);
            SetText(text);
            touchable = Touchable.Disabled;
        }

        public void SetWrap(bool wrap)
        {
            _wrapText = wrap;
        }

        public virtual Label SetStyle(LabelStyle style)
        {
            _style = style;
            InvalidateHierarchy();
            return this;
        }

        public virtual LabelStyle GetStyle() => _style;

        public Label SetText(string text)
        {
            if (_text != text)
            {
                _wrappedString = null;
                _text = text;
                _prefSizeInvalid = true;
                InvalidateHierarchy();
            }
            return this;
        }

        public string GetText() => _text;

        public void SetEllipsis(bool ellipsis)
        {
            if (ellipsis)
                _ellipsis = "...";
            else
                _ellipsis = null;
        }

        public int GetAlign()
        {
            return labelAlign;
        }
        
		public Label SetAlign(Align align)
        {
            return SetAlign(align, align);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labelAlign"></param>
        /// <param name="lineAlign">TODO align inside lines</param>
        /// <returns></returns>
        public Label SetAlign(Align labelAlign, Align lineAlign)
        {
            this.labelAlign = (int) labelAlign;

            // TODO
            //			var tempLineAlign = (int)lineAlign;
            //			if( ( tempLineAlign & AlignInternal.left ) != 0 )
            //				this.lineAlign = AlignInternal.left;
            //			else if( ( tempLineAlign & AlignInternal.right ) != 0 )
            //				this.lineAlign = AlignInternal.right;
            //			else
            //				this.lineAlign = AlignInternal.center;

            Invalidate();
            return this;
        }

        void ComputePrefSize()
        {
            _prefSizeInvalid = false;

            if (_wrapText && _ellipsis == null && width > 0)
            {
                var widthCalc = width;
                if (_style.background != null)
                    widthCalc -= _style.background.LeftWidth + _style.background.RightWidth;

                _wrappedString = _style.font.WrapText(_text, widthCalc / _fontScaleX);
            }
            else if (_ellipsis != null && width > 0)
            {
                // we have a max width and an ellipsis so we will truncate the text
                var widthCalc = width;
                if (_style.background != null)
                    widthCalc -= _style.background.LeftWidth + _style.background.RightWidth;

                _wrappedString = _style.font.TruncateText(_text, _ellipsis, widthCalc / _fontScaleX);
            }
            else
            {
                _wrappedString = _text;
            }

            _prefSize = _style.font.MeasureString(_wrappedString) * new Vector2(_fontScaleX, _fontScaleY);
        }

        public override void Layout()
        {
            if (_prefSizeInvalid)
                ComputePrefSize();

            var isWrapped = _wrapText && _ellipsis == null;
            if (isWrapped)
            {
                if (_lastPrefHeight != PreferredHeight)
                {
                    _lastPrefHeight = PreferredHeight;
                    InvalidateHierarchy();
                }
            }

            var width = this.width;
            var height = this.height;
            _textPosition.X = 0;
            _textPosition.Y = 0;
            // TODO: explore why descent causes mis-alignment
            //_textPosition.Y =_style.font.descent;
            if (_style.background != null)
            {
                _textPosition.X = _style.background.LeftWidth;
                _textPosition.Y = _style.background.TopHeight;
                width -= _style.background.LeftWidth + _style.background.RightWidth;
                height -= _style.background.TopHeight + _style.background.BottomHeight;
            }

            float textWidth, textHeight;
            if (isWrapped || _wrappedString.IndexOf('\n') != -1)
            {
                // If the text can span multiple lines, determine the text's actual size so it can be aligned within the label.
                textWidth = _prefSize.X;
                textHeight = _prefSize.Y;

                if ((labelAlign & AlignInternal.left) == 0)
                {
                    if ((labelAlign & AlignInternal.right) != 0)
                        _textPosition.X += width - textWidth;
                    else
                        _textPosition.X += (width - textWidth) / 2;
                }
            }
            else
            {
                textWidth = width;
                textHeight = _style.font.lineHeight * _fontScaleY;
            }

            if ((labelAlign & AlignInternal.bottom) != 0)
            {
                _textPosition.Y += height - textHeight;
                _textPosition.Y += _style.font.descent;
            }
            else if ((labelAlign & AlignInternal.top) != 0)
            {
                _textPosition.Y += 0;
                _textPosition.Y -= _style.font.descent;
            }
            else
            {
                _textPosition.Y += (height - textHeight) / 2;
            }

            //_textPosition.Y += textHeight;

            // if we have GlyphLayout this code is redundant
            if ((labelAlign & AlignInternal.left) != 0)
                _textPosition.X = 0;
            else if (labelAlign == AlignInternal.center)
                _textPosition.X = width / 2 - (_prefSize.X / 2); // center of width - center of text size
            else
                _textPosition.X = width - _prefSize.X; // full width - our text size
        }

        #region ILayout

        public override float PreferredWidth
        {
            get
            {
                if (_wrapText)
                    return 0;

                if (_prefSizeInvalid)
                    ComputePrefSize();

                var w = _prefSize.X;
                if (_style.background != null)
                    w += _style.background.LeftWidth + _style.background.RightWidth;
                return w;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                if (_prefSizeInvalid)
                    ComputePrefSize();

                var h = _prefSize.Y;
                if (_style.background != null)
                    h += _style.background.TopHeight + _style.background.BottomHeight;
                return h;
            }
        }

        public override void Invalidate()
        {
            base.Invalidate();
            _prefSizeInvalid = true;
        }

        #endregion

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            Validate();
            var color = new Color(this.color, (int)(this.color.A * parentAlpha));
            if (_style.background != null)
                _style.background.Draw(graphics, x, y, width == 0 ? _prefSize.X : width, height, color);

            _style.font.Draw(graphics, _wrappedString, (int)(x + _textPosition.X), (int)(y + _textPosition.Y), new Color(_style.fontColor, color.A));
            //graphics.DrawString(_style.font, _wrappedString, new Vector2(x, y) + _textPosition, _style.fontColor, 0, Vector2.Zero, new Vector2(_fontScaleX, _fontScaleY), SpriteEffects.None, 0);
        }
    }

    public class LabelStyle
    {
        public BitmapFont font;

        public Color fontColor;

        public IDrawable background;

        public LabelStyle(BitmapFont font, Color fontColor)
        {
            this.font = font;
            this.fontColor = fontColor;
        }

        public LabelStyle(LabelStyle style)
        {
            if (style != null)
            {
                font = style.font;
                fontColor = style.fontColor;
                background = style.background;
            }
        }

        public LabelStyle()
        {
            
        }
    }
}
