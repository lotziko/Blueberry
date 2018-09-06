using Microsoft.Xna.Framework;

namespace BlueberryCore.UI
{
    public class Separator : Element
    {
        private SeparatorStyle style;

        public Separator(Skin skin, string stylename = "default") : this(skin.Get<SeparatorStyle>(stylename)) { }

        public Separator(SeparatorStyle style)
        {
            this.style = style;
        }

        #region ILayout

        public override float PreferredWidth
        {
            get
            {
                return style.thickness;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                return style.thickness;
            }
        }

        #endregion

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            style.background.Draw(graphics, GetX(), GetY(), GetWidth(), GetHeight(), new Color(color, (int)(color.A * parentAlpha)));
        }

        public SeparatorStyle GetStyle()
        {
            return style;
        }
    }

    public class SeparatorStyle
    {
        public IDrawable background;
        public int thickness;

        public SeparatorStyle()
        {
        }

        public SeparatorStyle(SeparatorStyle style)
        {
            this.background = style.background;
            this.thickness = style.thickness;
        }

        public SeparatorStyle(IDrawable bg, int thickness)
        {
            this.background = bg;
            this.thickness = thickness;
        }
    }
}
