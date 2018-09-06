
using Microsoft.Xna.Framework;
using System;

namespace BlueberryCore.UI
{
    public class Image : Element
    {
        private float imageX, imageY, imageWidth, imageHeight;

        private Scaling _scaling;
        private IDrawable _drawable;
        private int _align;

        #region Constructors

        public Image(IDrawable drawable, Scaling scaling = Scaling.Stretch, Align align = Align.center)
        {
            SetDrawable(drawable);
            _scaling = scaling;
            _align = (int) align;
            SetSize(PreferredWidth, PreferredHeight);
            touchable = Touchable.Disabled;
        }

        public Image() : this(null)
        { }

        #endregion

        public Image SetDrawable(IDrawable drawable)
        {
            if (_drawable != drawable)
            {
                if (drawable != null)
                {
                    if (PreferredWidth != drawable.MinWidth || PreferredHeight != drawable.MinHeight)
                        InvalidateHierarchy();
                }
                else
                {
                    InvalidateHierarchy();
                }
                _drawable = drawable;
            }
            return this;
        }

        public Image SetAlign(Align align)
        {
            _align = (int) align;
            return this;
        }

        public Image SetScaling(Scaling scaling)
        {
            _scaling = scaling;
            return this;
        }

        public override void Layout()
        {
            if (_drawable == null)
                return;

            var regionWidth = _drawable.MinWidth;
            var regionHeight = _drawable.MinHeight;

            var size = _scaling.Apply(regionWidth, regionHeight, width, height);
            imageWidth = size.X;
            imageHeight = size.Y;

            if ((_align & AlignInternal.left) != 0)
                imageX = 0;
            else if ((_align & AlignInternal.right) != 0)
                imageX = (int)(width - imageWidth);
            else
                imageX = (int)(width / 2 - imageWidth / 2);

            if ((_align & AlignInternal.top) != 0)
                imageY = (int)(height - imageHeight);
            else if ((_align & AlignInternal.bottom) != 0)
                imageY = 0;
            else
                imageY = (int)(height / 2 - imageHeight / 2);
        }

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            Validate();

            //var col = new Color(color, color.A * parentAlpha);//var col = color * (color.A / 255f);

            if (_drawable != null)
            {
                //if (ClipBegin(graphics, x, y, width, height))
                {
                    var color = new Color(this.color, (int)(this.color.A * parentAlpha));
                    _drawable.Draw(graphics, x + imageX, y + imageY, imageWidth * scaleX, imageHeight * scaleY, color);
                    //ClipEnd(graphics);
                }
                
                //graphics.DrawRectangleBorder(x, y, width, height, Table.debugElementColor);
            }
        }

        #region ILayout

        public override float PreferredWidth
        {
            get { return _drawable != null ? _drawable.MinWidth : 0; }
        }

        public override float PreferredHeight
        {
            get { return _drawable != null ? _drawable.MinHeight : 0; }
        }

        #endregion
    }
}
