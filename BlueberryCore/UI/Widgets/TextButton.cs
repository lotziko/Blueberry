using Microsoft.Xna.Framework;
using System;

namespace BlueberryCore.UI
{
    public class TextButton : Button
    {
        private TextButtonStyle style;
        private Label label;

        public TextButton(string text, Skin skin, string stylename = "default") : this(text, skin.Get<TextButtonStyle>(stylename)) { }

        public TextButton(string text, TextButtonStyle style) : base(style)
        {
            SetStyle(style);
            this.style = style;
            label = new Label(text, style.font, style.fontColor);
            Add(label).Expand().Fill();
            SetSize(PreferredWidth, PreferredHeight);
        }

        public override void SetStyle(ButtonStyle style)
        {
            if (style == null)
                throw new ArgumentNullException("Style can't be null");
            if (!(style is TextButtonStyle))
                throw new ArgumentException("Can only pass TextButtonStyle");

            base.SetStyle(style);
            this.style = (TextButtonStyle)style;
            InvalidateHierarchy();
        }

        public Label GetLabel()
        {
            return label;
        }

        public Cell GetLabelCell()
        {
            return GetCell(label);
        }

        public void SetText(String text)
        {
            label.SetText(text);
        }

        public string GetText()
        {
            return label.GetText();
        }

        public new TextButtonStyle GetStyle() => style;

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            bool isOver = IsOver();
            bool isPressed = IsPressed();

            Color fontColor;
            if (isDisabled && style.disabledFontColor != Color.TransparentBlack)
                fontColor = style.disabledFontColor;
            else if (isPressed && isOver && style.downFontColor != Color.TransparentBlack)
                fontColor = style.downFontColor;
            else if (isChecked && style.checkedFontColor != Color.TransparentBlack)
                fontColor = (isOver && style.checkedOverFontColor != Color.TransparentBlack) ? style.checkedOverFontColor : style.checkedFontColor;
            else if (isOver && style.overFontColor != Color.TransparentBlack)
                fontColor = style.overFontColor;
            else
                fontColor = style.fontColor;
            if (fontColor != Color.TransparentBlack) label.GetStyle().fontColor = fontColor;
            base.Draw(graphics, parentAlpha);
        }
    }

    public class TextButtonStyle : ButtonStyle
    {
        public BitmapFont font;

        public Color fontColor, downFontColor, overFontColor, checkedFontColor, checkedOverFontColor, disabledFontColor;

        public TextButtonStyle() : base()
        {

        }

        public TextButtonStyle(IDrawable up, IDrawable down, BitmapFont font) : base(up, down)
        {
            this.font = font;
        }

        public TextButtonStyle(TextButtonStyle style) : base(style)
        {
            this.font = style.font;
            if (style.fontColor != null) this.fontColor = new Color(style.fontColor, style.fontColor.A);
            if (style.downFontColor != null) this.downFontColor = new Color(style.downFontColor, style.downFontColor.A);
            if (style.overFontColor != null) this.overFontColor = new Color(style.overFontColor, style.overFontColor.A);
            if (style.checkedFontColor != null) this.checkedFontColor = new Color(style.checkedFontColor, style.checkedFontColor.A);
            if (style.checkedOverFontColor != null) this.checkedOverFontColor = new Color(style.checkedOverFontColor, style.checkedOverFontColor.A);
            if (style.disabledFontColor != null) this.disabledFontColor = new Color(style.disabledFontColor, style.disabledFontColor.A);
        }
    }
}
