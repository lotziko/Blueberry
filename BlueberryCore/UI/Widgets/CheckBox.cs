using Microsoft.Xna.Framework;
using System;

namespace BlueberryCore.UI
{
    public class CheckBox : TextButton
    {
        private Image bgImage;
        private Image tickImage;
        private Cell imageStackCell;
        private Stack imageStack;
        private CheckBoxStyle style;

        public CheckBox(string text, Skin skin, string stylename = "default") : this(text, skin.Get<CheckBoxStyle>(stylename)) { }

        public CheckBox(string text, CheckBoxStyle style) : base(text, style)
        {
            ClearElements();

            bgImage = new Image(style.checkBackground);
            tickImage = new Image(style.tick);
            imageStackCell = Add(imageStack = new Stack(bgImage, tickImage));
            var label = GetLabel();
            Add(label).PadLeft(5);
            label.SetAlign(UI.Align.left);
            SetSize(PreferredWidth, PreferredHeight);
        }

        public override void SetStyle(ButtonStyle style)
        {
            if (!(style is CheckBoxStyle)) throw new ArgumentException("style must be a CheckBoxStyle.");
            base.SetStyle(style);
            this.style = (CheckBoxStyle)style;
        }

        public new CheckBoxStyle GetStyle() => style;

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            bgImage.SetDrawable(GetCheckboxBgImage());
            tickImage.SetDrawable(GetCheckboxTickImage());
            base.Draw(graphics, parentAlpha);

            /*if (isDisabled == false && stateInvalid && style.errorBorder != null)
            {
                style.errorBorder.draw(batch, getX() + imageStack.getX(), getY() + imageStack.getY(), imageStack.getWidth(), imageStack.getHeight());
            }*/
            /*if (focusBorderEnabled && drawBorder && style.focusedBorder != null)
            {
               
            }*/
        }

        protected override void DrawFocusBorder(Graphics graphics, float parentAlpha)
        {
            style.focusedBorder.Draw(graphics, GetX() + imageStack.GetX(), GetY() + imageStack.GetY(), imageStack.GetWidth(), imageStack.GetHeight(), new Color(color.R, color.G, color.B, (int)(color.A * parentAlpha)));
        }

        protected IDrawable GetCheckboxBgImage()
        {
            if (isDisabled) return style.checkBackground;
            if (IsPressed()) return style.checkBackgroundDown;
            if (IsOver()) return style.checkBackgroundOver;
            return style.checkBackground;
        }

        protected IDrawable GetCheckboxTickImage()
        {
            if (isChecked)
            {
                return isDisabled ? style.tickDisabled : style.tick;
            }
            return null;
        }

        public Image GetBackgroundImage()
        {
            return bgImage;
        }

        public Image GetTickImage()
        {
            return tickImage;
        }

        public Stack GetImageStack()
        {
            return imageStack;
        }

        public Cell GetImageStackCell()
        {
            return imageStackCell;
        }
        
    }

    public class CheckBoxStyle : TextButtonStyle
    {
        public IDrawable checkBackground;
        public IDrawable checkBackgroundOver;
        public IDrawable checkBackgroundDown;
        public IDrawable tick;
        public IDrawable tickDisabled;

        public CheckBoxStyle()
        {
            
        }
    }
}
