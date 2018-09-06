using BlueberryCore;
using BlueberryCore.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryEditor.UIElements
{
    public class Palette : Table
    {
        private PaletteStyle style;

        public Palette(PaletteStyle style)
        {
            SetStyle(style);
            SetSize(PreferredWidth, PreferredHeight);
            for(int i = 0; i < 8; i++)
            {
                Add(new PaletteElement(Color.White, this)).Pad(2f);
            }
        }

        public void SetStyle(PaletteStyle style)
        {
            this.style = style;
        }

        #region Subclasses

        public class PaletteElement : Element
        {
            protected Color paletteColor;
            protected Palette parent;

            public PaletteElement(Color paletteColor, Palette parent)
            {
                this.paletteColor = paletteColor;
                this.parent = parent;
                AddListener(new Listener(this));
            }

            public override void Draw(Graphics graphics, float parentAlpha)
            {
                graphics.DrawRectangle(GetX(), GetY(), GetWidth(), GetHeight(), new Color(paletteColor, (int)(paletteColor.A * parentAlpha)));
                graphics.DrawRectangleBorder(GetX(), GetY(), GetWidth(), GetHeight(), new Color(Color.Black, (GetColor().A * parentAlpha)));
            }

            #region ILayout

            public override float PreferredWidth
            {
                get
                {
                    return 16;
                }
            }

            public override float PreferredHeight
            {
                get
                {
                    return 16;
                }
            }

            #endregion

            #region Listener

            private class Listener : ClickListener
            {
                private readonly PaletteElement el;

                public Listener(PaletteElement el)
                {
                    this.el = el;
                }

                public override void Clicked(InputEvent ev, float x, float y)
                {
                    var picker = new ColorPicker(el.paletteColor, el.parent.style.pickerStyle);
                    picker.OnClose += (paletteColor) =>
                    {
                        el.paletteColor = paletteColor;
                    };
                    el.GetStage().AddElement(picker.FadeIn());
                    picker.CenterWindow();
                }
            }

            #endregion
        }

        #endregion

    }

    public class PaletteStyle
    {
        public ColorPickerStyle pickerStyle;

        public PaletteStyle(ColorPickerStyle pickerStyle)
        {
            this.pickerStyle = pickerStyle;
        }
    }
}
