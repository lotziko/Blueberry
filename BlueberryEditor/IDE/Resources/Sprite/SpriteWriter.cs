using BlueberryCore;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryEditor.IDE
{
    /// <summary>
    /// Is used to write a sprite to the disk, has .bb file containing config
    /// </summary>
    public class SpriteWriter : Writer<SpriteResource>
    {
        public override void Write(SpriteResource resource)
        {
            OnWriteStarted?.Invoke();

            if (!Directory.Exists(resource.Path))
                Directory.CreateDirectory(resource.Path);

            if (resource.Content.Name == null)
                throw new Exception("Sprite name can't be null");

            using (var sw = File.CreateText(resource.Path + resource.Content.Name + ".bb"))
            {
                sw.Write(JsonConvert.SerializeObject(resource.Content, Formatting.Indented));
            }
            var textures = resource.Content.Textures;

            if (textures != null)
            {
                for (int i = 0; i < textures.Length; i++)
                {
                    var data = new Color[textures[i].Width * textures[i].Height];
                    textures[i].GetData(data);
                    for(int j = 0; j < data.Length; j++)
                    {
                        data[j] = data[j].FromPremultiplied();
                    }
                    textures[i].SetData(data);
                    textures[i].SaveAsPng(File.Create(resource.Path + "_" + i + ".png"), resource.Content.Width, resource.Content.Height);
                }
            }
            GC.Collect();

            OnWriteEnded?.Invoke();
        }
    }
}
