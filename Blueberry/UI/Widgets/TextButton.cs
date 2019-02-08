using System;

namespace Blueberry.UI
{
    public partial class TextButton : Button
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

        protected override void RefreshBackground()
        {
            base.RefreshBackground();
            bool isOver = IsOver();
            bool isPressed = IsPressed();

            Col fontColor;
            if (isDisabled && style.disabledFontColor != Col.Transparent)
                fontColor = style.disabledFontColor;
            else if (isPressed && isOver && style.downFontColor != Col.Transparent)
                fontColor = style.downFontColor;
            else if (isChecked && style.checkedFontColor != Col.Transparent)
                fontColor = (isOver && style.checkedOverFontColor != Col.Transparent) ? style.checkedOverFontColor : style.checkedFontColor;
            else if (isOver && style.overFontColor != Col.Transparent)
                fontColor = style.overFontColor;
            else
                fontColor = style.fontColor;
            if (fontColor != label.GetColor())
            {
                label.GetStyle().fontColor = fontColor;
                Render.Request();
            }
        }

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            base.Draw(graphics, parentAlpha);
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

        public new TextButtonStyle GetStyle() => style;

        public Label GetLabel()
        {
            return label;
        }

        public Cell GetLabelCell()
        {
            return GetCell(label);
        }

        public void SetText(string text)
        {
            label.SetText(text);
        }

        public string GetText()
        {
            return label.GetText();
        }
    }


    public class TextButtonStyle : ButtonStyle
    {
        public IFont font;

        public Col fontColor, downFontColor, overFontColor, checkedFontColor, checkedOverFontColor, disabledFontColor;

        public TextButtonStyle() : base()
        {

        }

        public TextButtonStyle(IDrawable up, IDrawable down, IFont font) : base(up, down, null)
        {
            this.font = font;
        }

        public TextButtonStyle(TextButtonStyle style) : base(style)
        {
            this.font = style.font;
            if (style.fontColor != null) this.fontColor = new Col(style.fontColor);
            if (style.downFontColor != null) this.downFontColor = new Col(style.downFontColor);
            if (style.overFontColor != null) this.overFontColor = new Col(style.overFontColor);
            if (style.checkedFontColor != null) this.checkedFontColor = new Col(style.checkedFontColor);
            if (style.checkedOverFontColor != null) this.checkedOverFontColor = new Col(style.checkedOverFontColor);
            if (style.disabledFontColor != null) this.disabledFontColor = new Col(style.disabledFontColor);
        }
    }
}
