using Blueberry;
using System.Collections.Generic;

namespace BlueberryTexturePackerCore
{
    public class AtlasModel
    {
        public TextureAtlas Atlas { get; set; }
        public List<PageModel> Pages { get; set; }
        public string Path { get; }

        public AtlasModel(string path)
        {
            Path = path;
            Atlas = Content.Load<TextureAtlas>(path);
            Pages = new List<PageModel>();
            for(int i = 0; i < Atlas.PageCount; i++)
            {
                var page = new PageModel(this, i);
                Pages.Add(page);
            }
        }
    }
}
