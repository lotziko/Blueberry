using Blueberry;
using System;

namespace BlueberryTexturePackerCore
{
    public class PackerPreviewModel
    {
        public TextureAtlas Atlas
        {
            get => atlas;
            set
            {
                atlas = value;
                SetPage(0, atlas.PageCount);
            }
        }

        public event Action<int> OnZoomChanged;
        public event Action<(int w, int h)> OnDimensionsChanged;
        public event Action<float> OnDiskSizeChanged;
        public event Action<(int curr, int max)> OnPageChanged;

        private int zoom;
        private float diskSize;
        private (int w, int h) dimensions;
        private (int curr, int max) page;
        private TextureAtlas atlas;

        public void SetZoom(int value)
        {
            zoom = value;
            OnZoomChanged?.Invoke(zoom);
        }

        public void SetDiskSize(int value)
        {
            diskSize = value;
            OnDiskSizeChanged?.Invoke(value);
        }

        public void SetWidth(int w)
        {
            dimensions.w = w;
            OnDimensionsChanged?.Invoke(dimensions);
        }

        public void SetHeight(int h)
        {
            dimensions.h = h;
            OnDimensionsChanged?.Invoke(dimensions);
        }

        public void SetDimensions(int w, int h)
        {
            dimensions.w = w;
            dimensions.h = h;
            OnDimensionsChanged?.Invoke(dimensions);
        }

        public void SetPage(int value)
        {
            if (value >= 0 && value < page.max)
            {
                page.curr = value;
                OnPageChanged?.Invoke(page);
                SetDimensions(atlas.GetTexture(value).Width, atlas.GetTexture(value).Height);
            }
        }

        public void SetPage(int curr, int max)
        {
            page.curr = curr;
            page.max = max;
            OnPageChanged?.Invoke(page);
            SetDimensions(atlas.GetTexture(curr).Width, atlas.GetTexture(curr).Height);
        }

        public void NextPage()
        {
            if (page.curr < page.max - 1)
            {
                ++page.curr;
                SetPage(page.curr);
            }
        }

        public void PreviousPage()
        {
            if (page.curr > 0)
            {
                --page.curr;
                SetPage(page.curr);
            }
        }
    }
}
