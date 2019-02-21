using Blueberry;
using Blueberry.UI;

namespace OpenTKTest
{
    public class AtlasPackerTable : Table
    {
        public AtlasPackerTable(AtlasController controller, Skin skin)
        {
            Add(new AtlasPreviewTable(controller, skin));
        }

        public class AtlasPreviewTable : Table
        {
            private InfoPanel info;
            private TextureAtlasViewer preview;
            private ZoomableArea zoomableArea;
            private ImageButton previous, next;

            public AtlasPreviewTable(AtlasController controller, Skin skin)
            {
                var test = new DirectoryTable(skin);
                Add(test).Row();

                info = new InfoPanel(controller, skin);
                Add(info).Right().Row();

                preview = new TextureAtlasViewer(skin);
                preview.Atlas = controller.Atlas;

                zoomableArea = new ZoomableArea(preview, true);
                zoomableArea.OnZoomChanged += (value) =>
                {
                    controller.PreviewController.zoom.Value = value;
                };
                zoomableArea.SetBackground(new CheckerboardDrawable(new Col(77, 77, 77, 255), new Col(84, 84, 84, 255), 8));
                Add(zoomableArea).Size(600);

                controller.OnAtlasChanged += (atlas) =>
                {
                    if (atlas.PageCount == 1)
                    {
                        previous.SetDisabled(true);
                        next.SetDisabled(true);
                    }
                    else
                    {
                        previous.SetDisabled(false);
                        next.SetDisabled(false);
                    }
                };

                previous = new ImageButton(skin.GetDrawable("icon-arrow-left-circle"), skin);
                previous.SetGenerateDisabledImage(true);
                previous.OnClicked += () =>
                {
                    var (curr, max) = controller.PreviewController.page.Value;
                    if (curr > 1)
                    {
                        controller.PreviewController.page.Value = (--curr, max);
                        ChangePage(curr);
                    }
                };
                info.Add(previous);
                
                next = new ImageButton(skin.GetDrawable("icon-arrow-right-circle"), skin);
                next.SetGenerateDisabledImage(true);
                next.OnClicked += () =>
                {
                    var (curr, max) = controller.PreviewController.page.Value;
                    if (curr < max)
                    {
                        controller.PreviewController.page.Value = (++curr, max);
                        ChangePage(curr);
                    }
                };
                info.Add(next).Row();

                controller.ForceChange();
            }

            private void ChangePage(int page)
            {
                preview.ChangePage(page - 1);
                zoomableArea.FitContentAtCenter();
            }

            private class InfoPanel : Table
            {
                private Label dimensionsLabel, zoomLabel, pageLabel, diskMemoryLabel;
                private int pagesAmount, currentPage;

                public InfoPanel(AtlasController controller, Skin skin)
                {
                    dimensionsLabel = CreateLabelIcon(skin, "icon-dimensions");
                    zoomLabel = CreateLabelIcon(skin, "icon-zoom");
                    pageLabel = CreateLabelIcon(skin, "icon-pages");
                    diskMemoryLabel = CreateLabelIcon(skin, "icon-save");

                    controller.PreviewController.size.OnChange += (value) =>
                    {
                        var (w, h) = ((int w, int h))value;
                        SetDimensions(w, h);
                    };
                    controller.PreviewController.zoom.OnChange += (value) =>
                    {
                        SetZoomLevel((int)value);
                    };
                    controller.PreviewController.page.OnChange += (value) =>
                    {
                        var (curr, max) = ((int curr, int max))value;
                        SetCurrentPage(curr);
                        SetPagesAmount(max);
                    };
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
                    pageLabel.SetText(currentPage + "/" + pagesAmount);
                }
            }
        }
    }
}
