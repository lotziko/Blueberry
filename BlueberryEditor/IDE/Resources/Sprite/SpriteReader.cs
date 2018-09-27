using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.IO;

namespace BlueberryEditor.IDE
{
    public class SpriteReader : Reader<SpriteResource>
    {
        public override SpriteResource Read(string path)
        {
            OnReadStarted?.Invoke();

            SpriteResource resource = null;
            if (Path.HasExtension(path) && Path.GetExtension(path) == ".bb")
            {
                if (!File.Exists(path))
                    throw new FileLoadException("Path does not exists");

                using (var sr = File.OpenText(path))
                {
                    resource = new SpriteResource(path, JsonConvert.DeserializeObject<SpriteData>(sr.ReadToEnd()));
                }
            }
            else
            {
                if (!Directory.Exists(path))
                    throw new FileLoadException("Path does not exists");

                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    if (Path.GetExtension(file) == ".bb")
                    {
                        using (var sr = File.OpenText(file))
                        {
                            resource = new SpriteResource(path, JsonConvert.DeserializeObject<SpriteData>(sr.ReadToEnd()));
                            break;
                        }
                    }
                }
            }
                
            if (resource == null)
                throw new FileLoadException("Config file not found");

            var textures = new Texture2D[resource.Content.FrameCount];
            for(int i = 0; i < resource.Content.FrameCount; i++)
            {
                var filename = path + "_" + i + ".png";
                if (!File.Exists(filename))
                    throw new FileLoadException("Texture does not exists");

                using (var fs = new FileStream(filename, FileMode.Open))
                {
                    textures[i] = Texture2D.FromStream(BlueberryCore.Core.graphicsDevice, fs);
                }
            }
            resource.Content.SetTextures(textures);

            OnReadEnded?.Invoke();

            return resource;
        }
    }
}
