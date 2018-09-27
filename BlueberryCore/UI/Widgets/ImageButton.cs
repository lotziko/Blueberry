﻿using Microsoft.Xna.Framework;
using System;

namespace BlueberryCore.UI
{
    public class ImageButton : Button
    {
        protected Image image;
        private ImageButtonStyle style;
        private bool generateDisabledImage = false;

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
            if (style.imageDisabled == null)
                generateDisabledImage = true;
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
            if (generateDisabledImage && style.imageDisabled == null && IsDisabled())
                image.SetColor(Color.Gray);
            else
                image.SetColor(Color.White);
        }

        public bool IsGenerateDisabledImage()
        {
            return generateDisabledImage;
        }

        /**
         * @param generate when set to true and button state is set to disabled then button image will be tinted with gray
         * color to better symbolize that button is disabled. This works best for white images.
         */
        public void SetGenerateDisabledImage(bool generate)
        {
            generateDisabledImage = generate;
        }
    }

    public class ImageButtonStyle : ButtonStyle
    {
        public IDrawable imageUp, imageDown, imageOver, imageChecked, imageCheckedOver, imageDisabled;

        public ImageButtonStyle()
        {
        }

        public ImageButtonStyle(IDrawable up, IDrawable down, IDrawable checkked, IDrawable imageUp, IDrawable imageDown, IDrawable imageChecked) : base(up, down, checkked)
        {
            this.imageUp = imageUp;
            this.imageDown = imageDown;
            this.imageChecked = imageChecked;
        }

        public ImageButtonStyle(ImageButtonStyle style) : base(style)
        {
            this.imageUp = style.imageUp;
            this.imageDown = style.imageDown;
            this.imageOver = style.imageOver;
            this.imageChecked = style.imageChecked;
            this.imageCheckedOver = style.imageCheckedOver;
            this.imageDisabled = style.imageDisabled;
        }

        public ImageButtonStyle(ButtonStyle style) : base(style)
        {
        }
    }
}
