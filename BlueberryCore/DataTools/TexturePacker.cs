using BlueberryCore.DataTools;
using BlueberryCore.TextureAtlases;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace BlueberryCore.DataTools
{
    public abstract class TexturePacker
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        
        protected GraphicsDevice _graphicsDevice;

        protected virtual void Clear()
        {
            Width = 0;
            Height = 0;
        }

        public void Pack(string input, string output, string filename = "test", bool recursive = false, FileType type = FileType.BlueberryAtlas, int? preferredWidth = null, int? preferredHeight = null, int? trimOffset = null)
        {
            Clear();
            var data = ProcessRegionsAfterLoad(LoadTexturesFromDirectory(input, recursive), trimOffset);
            data = CalculateRectangles(data, preferredWidth, preferredHeight);
            var texture = Render(data);
            switch (type)
            {
                case FileType.BlueberryAtlas:
                    WriteBBAToFile(output, filename, data, texture);
                    break;
                case FileType.PNG:
                    for (int i = 0, n = texture.Length; i < n; i++)
                    {
                        var file = File.Create(output + "/" + filename + (n > 1 ? i + "" : "") + ".png");
                        texture[i].SaveAsPng(file, texture[i].Width, texture[i].Height);
                        file.Close();
                    }
                    break;
            }
            for (int i = 0, n = texture.Length; i < n; i++)
            {
                texture[i].Dispose();
            }
            GC.Collect();
        }

        public TextureAtlas Pack(string input, bool recursive = false, int? preferredWidth = null, int? preferredHeight = null, int? trimOffset = null)
        {
            Clear();
            var data = ProcessRegionsAfterLoad(LoadTexturesFromDirectory(input, recursive), trimOffset);
            data = CalculateRectangles(data, preferredWidth, preferredHeight);
            var result = new TextureAtlas(Render(data));
            foreach(PackerRegionData d in data)
            {
                result._regions.Add(d._region);
            }
            return result;
        }
        
        /// <summary>
        /// Main method that must be overriden
        /// </summary>
        /// <param name="textures"></param>
        /// <returns></returns>

        internal abstract List<PackerRegionData> CalculateRectangles(List<PackerRegionData> data, int? preferredWidth = null, int? preferredHeight = null);

        #region Render

        private Texture2D[] Render(List<PackerRegionData> data)
        {
            var pageCount = 1;
            foreach(PackerRegionData prd in data)
            {
                if (prd._region.page + 1 > pageCount)
                    pageCount = prd._region.page + 1;
            }
            var result = new Texture2D[pageCount];

            var batch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(_graphicsDevice);
            
            for(int i = 0; i < pageCount; i++)
            {
                var renderTarget = new RenderTarget2D(_graphicsDevice, Width, Height);

                _graphicsDevice.SetRenderTarget(renderTarget);
                _graphicsDevice.Clear(Color.Transparent);

                batch.Begin();
                foreach (PackerRegionData p in data)
                {
                    if (p._region.page == i)
                    {
                        var r = p._region;
                        batch.Draw(p._texture, new Rectangle(r.left, r.top, r.width, r.height), Color.White);
                    }
                }
                batch.End();
                result[i] = renderTarget;
            }

            _graphicsDevice.SetRenderTarget(null);
            foreach (PackerRegionData p in data)
            {
                p._texture.Dispose();
            }
            return result;
        }

        #endregion

        #region Write to file

        private void WriteBBAToFile(string path, string name, List<PackerRegionData> data, Texture2D[] texture)
        {
            var file = File.Create(path + "/" + name + ".bba");
            var writer = new BinaryWriter(file);
            var count = data.Count;

            writer.Write(count);
            for(int i = 0; i < count; i++)
            {
                var region = data[i]._region;
                writer.Write(region.name);

                writer.Write(region.rotate);

                writer.Write(region.left);
                writer.Write(region.top);
                writer.Write(region.width);
                writer.Write(region.height);

                //splits
                if (region.splits != null)
                {
                    writer.Write(true);
                    var splits = region.splits;
                    writer.Write(splits[0]);
                    writer.Write(splits[1]);
                    writer.Write(splits[2]);
                    writer.Write(splits[3]);
                }
                else
                    writer.Write(false);

                //pads
                if (region.pads != null)
                {
                    writer.Write(true);
                    var pads = region.pads;
                    writer.Write(pads[0]);
                    writer.Write(pads[1]);
                    writer.Write(pads[2]);
                    writer.Write(pads[3]);
                }
                else
                    writer.Write(false);

                writer.Write(region.originalWidth);
                writer.Write(region.originalHeight);

                writer.Write(region.offsetX);
                writer.Write(region.offsetY);

                writer.Write(region.index);
            }

            var textureCount = texture.Length;
            writer.Write(textureCount);
            writer.Write(texture[0].Width);
            writer.Write(texture[0].Height);

            for (int i = 0; i < textureCount; i++)
            {
                var tex = texture[i];
                var bytes = new byte[tex.Width * tex.Height * 4];
                tex.GetData(bytes);
                var compressed = Data.Compress(bytes);
                writer.Write(compressed.Length);
                writer.Write(compressed);
                tex.Dispose();
            }

            writer.Close();
            file.Close();
        }

        #endregion

        #region Get from files

        internal Dictionary<String, Texture2D> LoadTexturesFromDirectory(String path, bool recursive = false)
        {
            var result = new Dictionary<String, Texture2D>();
            var filePathes = Directory.GetFiles(path, "*.png", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            foreach (String filePath in filePathes)
            {
                var clearedPath = filePath.Replace(path + "\\", "").Replace(".png", "").Replace("\\", "/");
                result.Add(clearedPath, LoadTexture(filePath));
            }
            return result;
        }

        internal List<PackerRegionData> ProcessRegionsAfterLoad(Dictionary<String, Texture2D> data, int? trimOffset = 0)
        {
            var result = new List<PackerRegionData>();
            
            foreach (KeyValuePair<String, Texture2D> p in data)
            {
                var texture = p.Value;

                var region = new Region
                {
                    name = p.Key,
                    width = texture.Width,
                    height = texture.Height
                };

                int underscoreIndex = p.Key.IndexOf('_');
                if (underscoreIndex != -1 && char.IsDigit(p.Key[underscoreIndex + 1]))
                {
                    region.index = int.Parse(p.Key.Substring(underscoreIndex + 1));
                    region.name = region.name.Substring(0, underscoreIndex);
                }

                var colorArray = Data.GetColorArrayFromTexture(texture);

                int nineIndex = p.Key.IndexOf(".9");
                if (nineIndex != -1)
                {
                    region.splits = NinePatch.GetSplits(colorArray);
                    region.pads = NinePatch.GetPads(colorArray, region.splits);
                    if (region.width == 3 && region.height == 3)
                        region.splits = new int[] { 0, 0, 0, 0 };
                    region.name = region.name.Substring(0, nineIndex);

                    var newBounds = new Rectangle(1, 1, region.width - 2, region.height - 2);//new Rectangle(region.splits[0], region.splits[2], region.pads[0], region.pads[2]);
                    Color[] texData = new Color[newBounds.Width * newBounds.Height];
                    texture.GetData(0, newBounds, texData, 0, newBounds.Width * newBounds.Height);
                    var newTex = new Texture2D(_graphicsDevice, newBounds.Width, newBounds.Height);
                    newTex.SetData(texData);
                    texture.Dispose();
                    texture = newTex;
                    region.width = newBounds.Width;
                    region.height = newBounds.Height;
                    region.originalWidth = texture.Width;
                    region.originalHeight = texture.Height;
                }
                else if (trimOffset.HasValue)
                {
                    var trim = Data.CalculateTextureTrim(colorArray, trimOffset.Value);
                    var newBounds = new Rectangle(trim[0], trim[2], texture.Width - trim[0] - trim[1], texture.Height - trim[2] - trim[3]);

                    if (newBounds.Width < 0 || newBounds.Height < 0)
                    {
                        newBounds = new Rectangle(0, 0, 1, 1);
                    }

                    region.width = newBounds.Width;
                    region.height = newBounds.Height;
                    region.offsetX = newBounds.X;
                    region.offsetY = newBounds.Y;
                    region.originalWidth = texture.Width;
                    region.originalHeight = texture.Height;

                    Color[] texData = new Color[newBounds.Width * newBounds.Height];
                    texture.GetData(0, newBounds, texData, 0, newBounds.Width * newBounds.Height);
                    var newTex = new Texture2D(_graphicsDevice, newBounds.Width, newBounds.Height);
                    newTex.SetData(texData);
                    texture.Dispose();
                    texture = newTex;
                }
                var dataRegion = new PackerRegionData(texture, region);
                result.Add(dataRegion);
            }

            return result;
        }

        internal Texture2D LoadTexture(String path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Open);
            Texture2D result = Texture2D.FromStream(_graphicsDevice, fileStream);
            fileStream.Dispose();
            return result;
        }

        #endregion

        #region Constructor

        public TexturePacker(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        #endregion

    }

    internal struct PackerRegionData
    {
        public Texture2D _texture;
        public Region _region;

        public PackerRegionData(Texture2D texture, Region region)
        {
            _texture = texture;
            _region = region;
        }
    }

    public enum FileType
    {
        PNG, BlueberryAtlas
    }
}
