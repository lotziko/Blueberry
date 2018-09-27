using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryEditor.IDE
{
    public class SpriteResource : Resource<SpriteData>
    {
        public SpriteResource(string path, SpriteData content) : base(path, content)
        {
        }

        public override string ResourceTypeName { get { return "BBSprite"; } }

        public override string ToString()
        {
            return base.ToString() + " " + Content.Name;
        }
    }

    public class SpriteData : Data
    {
        [JsonIgnore]
        public Texture2D[] Textures { get; protected set; }
        [JsonProperty]
        public int Width { get; protected set; }
        [JsonProperty]
        public int Height { get; protected set; }
        [JsonProperty]
        public int OriginX { get; set; }
        [JsonProperty]
        public int OriginY { get; set; }
        [JsonProperty]
        public int FrameCount { get; protected set; }
        [JsonProperty]
        public int[] CollisionBox { get; protected set; } = new int[4] { 0, 0, 0, 0 };

        public SpriteData(string name, params Texture2D[] textures)
        {
            Name = name;
            SetTextures(textures);
        }

        public void SetTextures(params Texture2D[] textures)
        {
            if (textures != null)
            {
                if (textures.Length > 0)
                {
                    Texture2D first = null;
                    for (int i = 0; i < textures.Length; i++)
                    {
                        if (textures[i] != null)
                        {
                            first = textures[i];
                            break;
                        }
                    }
                    if (first == null)
                        throw new Exception("There is no texture in array");
                    for (int i = 0; i < textures.Length; i++)
                    {
                        if (textures[i].Width != first.Width || textures[i].Height != first.Height)
                            throw new Exception("Textures has different sizes");
                    }
                    Textures = textures;
                    Width = first.Width;
                    Height = first.Height;
                }
                else
                {
                    Textures = null;
                }
                FrameCount = textures.Length;
            }
            else
            {
                Textures = null;
            }
        }

        public void SetCollisionBox(int left, int top, int right, int bottom)
        {
            CollisionBox[0] = left;
            CollisionBox[1] = top;
            CollisionBox[2] = right;
            CollisionBox[3] = bottom;
        }
    }
}
