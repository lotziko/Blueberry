using Blueberry;
using Blueberry.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlueberryTexturePackerCore
{
    public class AtlasPreview : Table
    {
        #region Fields

        public event Action<AtlasModel> OnModelChanged;

        private AtlasModel model;
        public AtlasModel AtlasModel
        {
            get => model;
            set
            {
                model = value;
                OnModelChanged?.Invoke(model);
                SetPage(0);
                RefreshButtons();
            }
        }

        private InfoPanel info;
        private ZoomableArea zoom;
        private PreviewElement preview;
        private ImageButton previous, next;

        #endregion

        public AtlasPreview(Skin skin)
        {
            info = new InfoPanel(skin);
            Add(info).Right().Row();

            preview = new PreviewElement(null, skin.Get<AtlasPreviewStyle>());

            zoom = new ZoomableArea(preview);
            zoom.SetBackground(new CheckerboardDrawable(new Col(77, 77, 77, 255), new Col(84, 84, 84, 255), 8));

            Add(zoom).Expand().Fill();

            previous = new ImageButton(skin.GetDrawable("icon-arrow-left-circle"), skin);
            previous.SetGenerateDisabledImage(true);
            previous.OnClicked += () =>
            {
                PreviousPage();
            };
            info.Add(previous);

            next = new ImageButton(skin.GetDrawable("icon-arrow-right-circle"), skin);
            next.SetGenerateDisabledImage(true);
            next.OnClicked += () =>
            {
                NextPage();
            };
            info.Add(next).Row();
        }

        private void NextPage()
        {
            var index = model.Pages.IndexOf(preview.PageModel);
            if (index < model.Pages.Count - 1)
                SetPage(index + 1);
        }

        private void PreviousPage()
        {
            var index = model.Pages.IndexOf(preview.PageModel);
            if (index > 0)
                SetPage(index - 1);
        }

        private void SetPage(int index)
        {
            preview.PageModel = model.Pages[index];
        }

        private void RefreshButtons()
        {
            if (model.Pages.Count > 1)
            {
                if (previous.IsDisabled())
                {
                    previous.SetDisabled(false);
                    next.SetDisabled(false);
                }
            }
            else
            {
                if (!previous.IsDisabled())
                {
                    previous.SetDisabled(true);
                    next.SetDisabled(true);
                }
            }
        }


        #region Subclasses

        private class PreviewElement : Element
        {
            public event Action<PageModel> OnModelChanged;

            private PageModel model;
            public PageModel PageModel
            {
                get => model;
                set
                {
                    model = value;
                    OnModelChanged?.Invoke(model);
                    if (model != null)
                    {
                        SetSize(model.Width, model.Height);
                    }
                    Invalidate();
                }
            }

            private Rect bounds, textBounds;
            private AtlasRegion cRegion;
            private AtlasPreviewStyle style;

            public PreviewElement(PageModel model, AtlasPreviewStyle style)
            {
                PageModel = model;
                this.style = style;
                AddListener(new Listener(this));
            }

            public override void Draw(Graphics graphics, float parentAlpha)
            {
                Validate();

                graphics.DrawRectangle(bounds, false, new Col(1.0f, 1.0f, 1.0f, 0.5f * parentAlpha));
                graphics.DrawRectangle(bounds, true, new Col(Col.Black));

                if (model == null)
                    return;

                graphics.DrawTexture(model.Texture, bounds.X, bounds.Y, bounds.Width, bounds.Height, new Col(GetColor(), GetColor().A * parentAlpha));

                if (cRegion != null)
                {
                    graphics.DrawRectangle(bounds.X + (cRegion.X - 1) * GetScaleX(), bounds.Y + (cRegion.Y - 1) * GetScaleY(), (cRegion.Width + 2) * GetScaleX(), (cRegion.Height + 2) * GetScaleY(), true, Col.Red);
                    if (style != null)
                    {
                        if (style.font != null)
                        {
                            graphics.DrawRectangle(textBounds, false, new Col(0.0f, 0.0f, 0.0f, 0.5f * parentAlpha));
                            style.font.Draw(graphics, cRegion.Name, textBounds.X + 6, textBounds.Y + 3);
                        }
                    }
                }
            }

            #region ILayout

            public override void Layout()
            {
                if (PageModel == null)
                    return;
                var tex = PageModel.Texture;
                bounds.Set(GetX(), GetY(), tex.Width * GetScaleX(), tex.Height * GetScaleY());
                if (cRegion != null && style != null && style.font != null)
                {
                    var measured = style.font.MeasureString(cRegion.Name);
                    int padding = 6;
                    textBounds.Set(bounds.X + (cRegion.X + (cRegion.Width - measured.X / GetScaleX()) / 2) * GetScaleX() - padding, bounds.Y + (cRegion.Y + cRegion.Height + 1) * GetScaleY(), measured.X + padding * 2, measured.Y + padding);
                }
            }

            #endregion

            #region Listener

            private class Listener : InputListener<PreviewElement>
            {
                public Listener(PreviewElement par) : base(par)
                {
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
                    AtlasRegion newRegion = null;
                    foreach (var r in par.PageModel.Regions)
                    {
                        if (x > r.X && y > r.Y && x < r.X + r.Width && y < r.Y + r.Height)
                        {
                            newRegion = r;
                            break;
                        }
                    }
                    if (newRegion != par.cRegion)
                    {
                        par.cRegion = newRegion;
                        Render.Request();
                    }
                }
            }

            #endregion
        }

        private class InfoPanel : Table
        {
            private Label dimensionsLabel, zoomLabel, pageLabel, diskMemoryLabel;
            private int pagesAmount, currentPage;

            public InfoPanel(Skin skin)
            {
                dimensionsLabel = CreateLabelIcon(skin, "icon-dimensions");
                zoomLabel = CreateLabelIcon(skin, "icon-zoom");
                pageLabel = CreateLabelIcon(skin, "icon-pages");
                diskMemoryLabel = CreateLabelIcon(skin, "icon-save");

                /*model.OnDimensionsChanged += (value) =>
                {
                    SetDimensions(value.w, value.h);
                };
                model.OnZoomChanged += (value) =>
                {
                    SetZoomLevel(value);
                };
                model.OnPageChanged += (value) =>
                {
                    SetCurrentPage(value.curr);
                    SetPagesAmount(value.max);
                };*/
            }

            private Label CreateLabelIcon(Skin skin, string iconName)
            {
                Add(new Image(skin.GetDrawable(iconName)));
                var label = new Label("", skin);
                Add(label).PadRight(4);
                return label;
            }

            public void SetZoomLevel(int level)
            {
                zoomLabel.SetText(level + "%");
            }

            public void SetDimensions(int width, int height)
            {
                dimensionsLabel.SetText(width + " x " + height);
            }

            public void SetPagesAmount(int amount)
            {
                pagesAmount = amount;
                UpdatePages();
            }

            public void SetCurrentPage(int page)
            {
                currentPage = page;
                UpdatePages();
            }

            private void UpdatePages()
            {
                pageLabel.SetText((currentPage + 1) + "/" + pagesAmount);
            }
        }

        #endregion
    }

    public class AtlasPreviewStyle
    {
        public IFont font;

        public AtlasPreviewStyle()
        {
        }

        public AtlasPreviewStyle(IFont font)
        {
            this.font = font;
        }
    }
}
