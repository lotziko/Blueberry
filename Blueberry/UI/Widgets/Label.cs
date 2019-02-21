using Blueberry.DataTools;

namespace Blueberry.UI
{
    public partial class Label : Element
    {
        protected string text;
        protected float fontScaleX = 1;
        protected float fontScaleY = 1;
        protected int labelAlign = AlignInternal.center;

        /// <summary>
        /// String is "..." when do not fit
        /// </summary>
        protected string ellipsis;
        protected bool wrapText;

        // internal state
        protected string wrappedString;
        protected bool prefSizeInvalid;
        protected float lastPrefHeight;
        protected Vec2 prefSize;
        protected Vec2 textPosition;

        protected LabelStyle style;

        #region Binding

        private IDataBinding binding;
        private object bindingData;

        public IDataBinding DataBinding
        {
            get => binding;
            set
            {
                if (binding != null)
                    binding.OnChange -= Binding_OnChange;

                binding = value;
                binding.OnChange += Binding_OnChange;
                binding.ForceChange();
            }
        }

        private void Binding_OnChange(object obj)
        {
            SetText(obj);
        }

        #endregion

        public Label(string text, IFont font, Col fontColor) : this(text, new LabelStyle(font, fontColor)) { }

        public Label(string text, Skin skin, string stylename = "default") : this(text, skin.Get<LabelStyle>(stylename)) { }

        public Label(string text, LabelStyle style)
        {
            SetStyle(style);
            SetText(text);
            touchable = Touchable.Disabled;
        }

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            Validate();
            var color = new Col(this.color, this.color.A * parentAlpha);
            if (style.background != null)
                style.background.Draw(graphics, x, y, width == 0 ? prefSize.X : width, height, color);

            style.font.Draw(graphics, wrappedString, (int)(x + textPosition.X), (int)(y + textPosition.Y), new Col(style.fontColor, color.A));
            //graphics.DrawString(_style.font, _wrappedString, new Vector2(x, y) + _textPosition, _style.fontColor, 0, Vector2.Zero, new Vector2(_fontScaleX, _fontScaleY), SpriteEffects.None, 0);
        }

        public virtual Label SetStyle(LabelStyle style)
        {
            this.style = style;
            InvalidateHierarchy();
            return this;
        }

        public virtual LabelStyle GetStyle() => style;

        public void SetWrap(bool wrap)
        {
            wrapText = wrap;
        }

        public Label SetText(object text)
        {
            if (binding != null && text != bindingData)
            {
                binding.Value = bindingData = text;
                SetText(text + "");
            }
            return this;
        }

        public virtual Label SetText(string text)
        {
            if (this.text != text)
            {
                wrappedString = null;
                this.text = text;
                prefSizeInvalid = true;
                InvalidateHierarchy();
            }
            return this;
        }

        public string GetText() => text;

        public void SetEllipsis(bool ellipsis)
        {
            if (ellipsis)
                this.ellipsis = "...";
            else
                this.ellipsis = null;
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
            this.labelAlign = (int)labelAlign;

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
            prefSizeInvalid = false;

            if (wrapText && ellipsis == null && width > 0)
            {
                var widthCalc = width;
                if (style.background != null)
                    widthCalc -= style.background.LeftWidth + style.background.RightWidth;

                wrappedString = style.font.WrapText(text, widthCalc / fontScaleX);
            }
            else if (ellipsis != null && width > 0)
            {
                // we have a max width and an ellipsis so we will truncate the text
                var widthCalc = width;
                if (style.background != null)
                    widthCalc -= style.background.LeftWidth + style.background.RightWidth;

                wrappedString = style.font.TruncateText(text, ellipsis, widthCalc / fontScaleX);
            }
            else
            {
                wrappedString = text;
            }

            prefSize = style.font.MeasureString(wrappedString) * new Vec2(fontScaleX, fontScaleY);
        }

        public override void Layout()
        {
            if (prefSizeInvalid)
                ComputePrefSize();

            var isWrapped = wrapText && ellipsis == null;
            if (isWrapped)
            {
                if (lastPrefHeight != PreferredHeight)
                {
                    lastPrefHeight = PreferredHeight;
                    InvalidateHierarchy();
                }
            }

            var width = this.width;
            var height = this.height;
            textPosition.X = 0;
            textPosition.Y = 0;
            // TODO: explore why descent causes mis-alignment
            //_textPosition.Y =_style.font.descent;
            if (style.background != null)
            {
                textPosition.X = style.background.LeftWidth;
                textPosition.Y = style.background.TopHeight;
                width -= style.background.LeftWidth + style.background.RightWidth;
                height -= style.background.TopHeight + style.background.BottomHeight;
            }

            float textWidth, textHeight;
            if (isWrapped || wrappedString.IndexOf('\n') != -1)
            {
                // If the text can span multiple lines, determine the text's actual size so it can be aligned within the label.
                textWidth = prefSize.X;
                textHeight = prefSize.Y;

                if ((labelAlign & AlignInternal.left) == 0)
                {
                    if ((labelAlign & AlignInternal.right) != 0)
                        textPosition.X += width - textWidth;
                    else
                        textPosition.X += (width - textWidth) / 2;
                }
            }
            else
            {
                textWidth = width;
                textHeight = style.font.LineHeight * fontScaleY;
            }

            if ((labelAlign & AlignInternal.bottom) != 0)
            {
                textPosition.Y += height - textHeight;
                textPosition.Y += style.font.Descent;
            }
            else if ((labelAlign & AlignInternal.top) != 0)
            {
                textPosition.Y += 0;
                textPosition.Y -= style.font.Descent;
            }
            else
            {
                textPosition.Y += (height - textHeight) / 2;
            }

            //_textPosition.Y += textHeight;

            if ((labelAlign & AlignInternal.left) != 0)
                textPosition.X += 0;
            else if (labelAlign == AlignInternal.center)
                textPosition.X += width / 2 - (prefSize.X / 2);
            else
                textPosition.X += width - prefSize.X;

            // if we have GlyphLayout this code is redundant
            /*if ((labelAlign & AlignInternal.left) != 0)
                textPosition.X = 0;
            else if (labelAlign == AlignInternal.center)
                textPosition.X = width / 2 - (prefSize.X / 2); // center of width - center of text size
            else
                textPosition.X = width - prefSize.X; // full width - our text size
            textPosition.X += style.background.LeftWidth;*/
        }

        #region ILayout

        public override float PreferredWidth
        {
            get
            {
                if (wrapText)
                    return 0;

                if (prefSizeInvalid)
                    ComputePrefSize();

                var w = prefSize.X;
                if (style.background != null)
                    w += style.background.LeftWidth + style.background.RightWidth;
                return w;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                if (prefSizeInvalid)
                    ComputePrefSize();

                var h = prefSize.Y;
                if (style.background != null)
                    h += style.background.TopHeight + style.background.BottomHeight;
                return h;
            }
        }

        public override void Invalidate()
        {
            base.Invalidate();
            prefSizeInvalid = true;
        }

        #endregion
    }

    public class LabelStyle
    {
        public IFont font;

        public Col fontColor;

        public IDrawable background;

        public LabelStyle(IFont font, Col fontColor)
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
