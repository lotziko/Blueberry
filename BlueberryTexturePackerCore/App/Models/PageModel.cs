using Blueberry;
using System;
using System.Collections.Generic;

namespace BlueberryTexturePackerCore
{
    public class PageModel : IDisposable
    {
        public AtlasModel AtlasModel { get; }
        public int PageIndex { get; }
        public Texture Texture { get; }
        public List<AtlasRegion> Regions { get; }

        public int Width => Texture.Width;
        public int Height => Texture.Height;

        public PageModel(AtlasModel atlasModel, int pageIndex)
        {
            AtlasModel = atlasModel;
            PageIndex = pageIndex;

            Texture = atlasModel.Atlas?.GetTexture(pageIndex);
            Regions = new List<AtlasRegion>();
            foreach(var r in atlasModel.Atlas?.Regions)
            {
                if (r.Texture == Texture)
                    Regions.Add(r);
            }
        }

        public void Dispose()
        {
            Texture?.Dispose();
        }
    }
}
