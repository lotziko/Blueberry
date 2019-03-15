using Blueberry;
using Blueberry.DataTools;
using System;
using System.Collections.Generic;

namespace BlueberryTexturePackerCore
{
    public class PackerWindowModel
    {
        public event Action<TextureAtlas> OnSelectedAtlasChanged;

        private string outputPath = "";
        public string OutputPath
        {
            set
            {
                outputPath = value;
            }
        }
        private readonly Settings settings = new Settings();
        
        public void ChangeSettingsBoolValue(string name, bool value)
        {
            typeof(Settings).GetField(name)?.SetValue(settings, value);
        }

        /*public void SelectAtlas(string path)
        {
            LoadAtlas(path);
            OnSelectedAtlasChanged?.Invoke(atlasModels[path].Atlas);
        }*/

        public void CreateAtlas(string name)
        {
            var model = new AtlasModel() { Name = name };
            atlasModels.Add(model);
        }

        public void PackAtlas()
        {
            //new TexturePacker(settings).Pack();
        }

        /*public void LoadAtlas(string path)
        {
            if (!atlasModels.ContainsKey(path))
            {
                var atlas = Content.Load<TextureAtlas>(path);
                atlasModels.Add(path, new AtlasModel() { Atlas = atlas, Path = path });
            }
        }*/
    }
}
