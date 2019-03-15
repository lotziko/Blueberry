using Blueberry;
using Blueberry.UI;

namespace BlueberryTexturePackerCore
{
    public class MainTable : Table
    {
        public MainTable(AtlasModel model, Skin skin)
        {
            var preview = new AtlasPreview(skin);
            preview.AtlasModel = model;
            Add(preview).Size(500);
            /*var previewModel = service.PackerPreviewModel;
            var windowModel = service.PackerWindowModel;
            */
            /*var info = new InfoPanel(previewModel, skin);
            Add(info).Right().Row();

            preview = new TextureAtlasViewer(skin);
            windowModel.OnSelectedAtlasChanged += (atlas) =>
            {
                preview.Atlas = atlas;
            };
            windowModel.SelectAtlas("Resources/UI.bba");

            zoomableArea = new ZoomableArea(preview);
            zoomableArea.OnZoomChanged += (value) =>
            {
                previewModel.SetZoom(value);
            };
            previewModel.OnPageChanged += (value) =>
            {
                zoomableArea.FitContentAtCenter();
            };
            zoomableArea.SetBackground(new CheckerboardDrawable(new Col(77, 77, 77, 255), new Col(84, 84, 84, 255), 8));
            Add(zoomableArea).Size(600);

            previewModel.OnPageChanged += (value) =>
            {
                preview.ChangePage(value.curr);
            };

            #region buttons

            var previous = new ImageButton(skin.GetDrawable("icon-arrow-left-circle"), skin);
            previous.SetGenerateDisabledImage(true);
            previous.OnClicked += () =>
            {
                previewModel.PreviousPage();
            };
            info.Add(previous);

            var next = new ImageButton(skin.GetDrawable("icon-arrow-right-circle"), skin);
            next.SetGenerateDisabledImage(true);
            next.OnClicked += () =>
            {
                previewModel.NextPage();
            };
            info.Add(next).Row();

            #endregion*/
        }
    }
}
