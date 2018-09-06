using System;

namespace BlueberryCore.UI
{
    public class ImageButton : Button
    {
        protected Image image;
        private ImageButtonStyle style;

        public ImageButton(IDrawable imageUp, Skin skin, string stylename = "default") : this(imageUp, skin.Get<ImageButtonStyle>(stylename))
        {
            
        }

        public ImageButton(ImageButtonStyle style) : this(style.imageUp, style)
        {

        }

        public ImageButton(IDrawable imageUp, ImageButtonStyle style) : base(style)
        {
            style.imageUp = imageUp;
            image = new Image();
            image.SetScaling(Scaling.Fit);
            Add(image);
            SetSize(PreferredWidth, PreferredHeight);
            UpdateImage();
        }

        /// <summary>
        /// Set ImageButton style
        /// </summary>
        /// <param name="style">Instance of ImageButtonStyle class</param>

        public override void SetStyle(ButtonStyle style)
        {
            if (!(style is ImageButtonStyle))
                throw new ArgumentException("Can only pass ImageButtonStyle");
            base.SetStyle(style);

            this.style = (ImageButtonStyle)style;
            InvalidateHierarchy();
        }

        public new ImageButtonStyle GetStyle() => style;

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            UpdateImage();
            base.Draw(graphics, parentAlpha);
        }

        public Image GetImage()
        {
            return image;
        }

        protected void UpdateImage()
        {
            IDrawable drawable = null;
            if (isDisabled && style.imageDisabled != null)
                drawable = style.imageDisabled;
            else if (IsPressed() && IsOver() && style.imageDown != null)
                drawable = style.imageDown;
            else if (isChecked && style.imageChecked != null)
                drawable = (style.imageCheckedOver != null && IsOver()) ? style.imageCheckedOver : style.imageChecked;
            else if (IsOver() && style.imageOver != null)
                drawable = style.imageOver;
            else if (style.imageUp != null)
                drawable = style.imageUp;
            image.SetDrawable(drawable);
        }
    }

    public class ImageButtonStyle : ButtonStyle
    {
        public IDrawable imageUp, imageDown, imageOver, imageChecked, imageCheckedOver, imageDisabled;
    }
}
