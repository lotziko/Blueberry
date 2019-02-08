using Blueberry.DataTools;
using System;

namespace Blueberry.UI
{
    public partial class CheckBox : TextButton
    {
        private Image bgImage;
        private Image tickImage;
        private Cell imageStackCell;
        private Stack imageStack;
        private CheckBoxStyle style;

        #region Binding

        private IDataBinding binding;

        public IDataBinding DataBinding
        {
            get => binding;
            set
            {
                if (binding != null)
                    binding.OnChange -= Binding_OnChange;

                binding = value;
                binding.OnChange += Binding_OnChange;
            }
        }

        private void Binding_OnChange(object obj)
        {
            if (obj is bool)
                SetChecked((bool)obj);
        }

        #endregion

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

        protected override void RefreshBackground()
        {
            base.RefreshBackground();
            var bg = GetCheckboxBgImage();
            var tick = GetCheckboxTickImage();
            if (bgImage.GetDrawable() != bg || tickImage.GetDrawable() != tick)
            {
                bgImage.SetDrawable(bg);
                tickImage.SetDrawable(tick);
                Render.Request();
            }
        }

        public override void Draw(Graphics graphics, float parentAlpha)
        {
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
            style.focusedBorder.Draw(graphics, GetX() + imageStack.GetX(), GetY() + imageStack.GetY(), imageStack.GetWidth(), imageStack.GetHeight(), new Col(color, color.A * parentAlpha));
        }

        public override void SetStyle(ButtonStyle style)
        {
            if (!(style is CheckBoxStyle)) throw new ArgumentException("style must be a CheckBoxStyle.");
            base.SetStyle(style);
            this.style = (CheckBoxStyle)style;
        }

        public new CheckBoxStyle GetStyle() => style;

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
