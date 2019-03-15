using Blueberry;
using Blueberry.DataTools;
using System;

namespace OpenTKTest
{
    public class AtlasController
    {
        private TextureAtlas atlas;
        public TextureAtlas Atlas
        {
            get => atlas;
            set
            {
                if (PreviewController != null)
                {
                    if (atlas == null)
                        PreviewController.page.OnChange += Page_OnChange;
                    atlas = value;
                    PreviewController.page.Value = (1, atlas.PageCount);
                }
                OnAtlasChanged?.Invoke(value);
            }
        }

        private IContentExporter exporter;
        public IContentExporter Exporter
        {
            get => exporter;
            set
            {
                if (exporter != value)
                    exporter = value;
            }
        }

        public string OutputDirectory { get; set; }

        public Settings AtlasSettings { get; set; } = new Settings();

        private void Page_OnChange(object obj)
        {
            var page = atlas.GetTexture(PreviewController.page.Value.curr - 1);
            PreviewController.size.Value = (page.Width, page.Height);
        }

        public event Action<TextureAtlas> OnAtlasChanged;

        public AtlasPreviewController PreviewController { get; set; } = new AtlasPreviewController();

        public AtlasController(TextureAtlas atlas)
        {
            Atlas = atlas;
        }

        public void ForceChange()
        {
            OnAtlasChanged?.Invoke(Atlas);
            PreviewController?.ForceChange();
        }

        public class AtlasPreviewController
        {
            public DataBinding<int> zoom = new DataBinding<int>();
            public DataBinding<(int w, int h)> size = new DataBinding<(int w, int h)>();
            public DataBinding<float> onDiskSize = new DataBinding<float>();
            public DataBinding<(int curr, int max)> page = new DataBinding<(int curr, int max)>();

            public void ForceChange()
            {
                zoom.ForceChange();
                size.ForceChange();
                onDiskSize.ForceChange();
                page.ForceChange();
            }
        }
    }
}
