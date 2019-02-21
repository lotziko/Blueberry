using Blueberry;
using Blueberry.UI;
using System.Collections.Generic;

namespace OpenTKTest
{
    public class TextureAtlasViewer : Element
    {
        private Rect bounds, textBounds;
        private TextureAtlas atlas;
        private List<Region> regions;
        private Region currentRegion;
        private int currentPage = 0;

        private TextureAtlasViewerStyle style;

        public TextureAtlas Atlas
        {
            get => atlas;
            set
            {
                atlas = value;
                regions = atlas.GetRegions();
                ChangePage(0);
            }
        }
        
        public bool ChangePage(int page)
        {
            if (page >= 0 && page < atlas.PageCount)
            {
                currentPage = page;
                var tex = atlas.GetTexture(page);
                SetSize(tex.Width, tex.Height);
                return true;
            }
            return false;
        }

        public TextureAtlasViewer(Skin skin, string stylename = "default") : this(skin.Get<TextureAtlasViewerStyle>(stylename)) { }

        public TextureAtlasViewer(TextureAtlasViewerStyle style)
        {
            this.style = style;
            AddListener(new Listener(this));
        }

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            Validate();

            graphics.DrawRectangle(bounds, false, new Col(1.0f, 1.0f, 1.0f, 0.5f * parentAlpha));
            graphics.DrawRectangle(bounds, true, new Col(Col.Black));
            graphics.DrawTexture(atlas.GetTexture(currentPage), bounds, new Col(GetColor(), GetColor().A * parentAlpha));

            if (currentRegion != null)
            {
                graphics.DrawRectangle(bounds.X + (currentRegion.X - 1) * GetScaleX(), bounds.Y + (currentRegion.Y - 1) * GetScaleY(), (currentRegion.Width + 2) * GetScaleX(), (currentRegion.Height + 2) * GetScaleY(), true, Col.Red);
                if (style != null)
                {
                    if (style.font != null)
                    {
                        graphics.DrawRectangle(textBounds, false, new Col(0.0f, 0.0f, 0.0f, 0.5f * parentAlpha));
                        style.font.Draw(graphics, currentRegion.Name, textBounds.X + 6, textBounds.Y + 3);
                    }
                }
            }
        }

        #region Listener

        private class Listener : InputListener
        {
            private readonly TextureAtlasViewer v;

            public Listener(TextureAtlasViewer v)
            {
                this.v = v;
            }

            public override bool MouseMoved(InputEvent ev, float x, float y)
            {
                FindRegion(x, y);
                return base.MouseMoved(ev, x, y);
            }

            public override bool Scrolled(InputEvent ev, float x, float y, float amountX, float amountY)
            {
                FindRegion(x, y);
                return base.Scrolled(ev, x, y, amountX, amountY);
            }

            public override void Enter(InputEvent ev, float x, float y, int pointer, Element fromElement)
            {
                FindRegion(x, y);
            }

            public override void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
            {
                FindRegion(-1, -1);
            }

            private void FindRegion(float x, float y)
            {
                Region newRegion = null;
                foreach(var r in v.regions)
                {
                    if (r.Page == v.currentPage && (x > r.X && y > r.Y && x < r.X + r.Width && y < r.Y + r.Height))
                    {
                        newRegion = r;
                        break;
                    }
                }
                if (newRegion != v.currentRegion)
                {
                    v.currentRegion = newRegion;
                    Render.Request();
                }
            }
        }

        #endregion

        #region ILayout

        //public override float PreferredWidth => bounds.Width;

        //public override float PreferredHeight => bounds.Height;

        public override void Layout()
        {
            var tex = atlas.GetTexture(currentPage);
            bounds.Set(GetX(), GetY(), tex.Width * GetScaleX(), tex.Height * GetScaleY());
            if (currentRegion != null && style != null && style.font != null)
            {
                var measured = style.font.MeasureString(currentRegion.Name);
                int padding = 6;
                textBounds.Set(bounds.X + (currentRegion.X + (currentRegion.Width - measured.X / GetScaleX()) / 2) * GetScaleX() - padding, bounds.Y + (currentRegion.Y + currentRegion.Height + 1) * GetScaleY(), measured.X + padding * 2, measured.Y + padding);
            }
        }

        #endregion
    }

    public class TextureAtlasViewerStyle
    {
        public IFont font;
    }
}
