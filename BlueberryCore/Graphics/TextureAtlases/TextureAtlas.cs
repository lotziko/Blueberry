using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BlueberryCore.TextureAtlases
{
    public class TextureAtlas : IDisposable
    {
        internal GraphicsDevice GraphicsDevice => Core.graphicsDevice;

        /// <summary>
        /// Holds one texture
        /// </summary>

        internal Texture2D[] texture;

        /// <summary>
        /// Holds texture regions accessed by name
        /// </summary>

        internal List<Region> _regions = new List<Region>();
        internal String assetName;
        internal int Count => _regions.Count;

        /// <summary>
        /// Get first region by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>

        public TextureRegion FindRegion(String name)
        {
            for (var i = 0; i < Count; i++)
            {
                if (_regions[i].GetName() == name)
                {
                    var region = _regions[i];
                    return new TextureRegion(texture[region.page], region.left, region.top, region.width, region.height, region.offsetX, region.offsetY);
                }
            }
            throw new KeyNotFoundException(name);
        }

        /// <summary>
        /// Get all regions of name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>

        public List<TextureRegion> FindRegions(String name)
        {
            var result = new List<TextureRegion>();
            for (var i = 0; i < Count; i++)
            {
                if (_regions[i].GetName() == name)
                {
                    var region = _regions[i];
                    result.Add(new TextureRegion(texture[region.page], region.left, region.top, region.originalWidth, region.originalHeight, region.offsetX, region.offsetY));
                }
            }
            return result;
        }

        /// <summary>
        /// Get all regions
        /// </summary>
        /// <returns></returns>

        public Dictionary<String, TextureRegion> FindRegions()
        {
            var result = new Dictionary<String, TextureRegion>();
            for (var i = 0; i < Count; i++)
            { 
                var region = _regions[i];
                result.Add(_regions[i].name, new TextureRegion(texture[region.page], region.left, region.top, region.originalWidth, region.originalHeight, region.offsetX, region.offsetY));
            }
            return result;
        }

        public NinePatch FindNinePatch(String name)
        {
            for (var i = 0; i < Count; i++)
            {
                if (_regions[i].GetName() == name)
                {
                    var region = _regions[i];
                    var textureRegion = new TextureRegion(texture[region.page], region.left, region.top, region.originalWidth, region.originalHeight);
                    if (region.splits == null)
                        throw new Exception("TextureRegion is not a ninepatch");
                    var ninepatch = new NinePatch(textureRegion, region.splits[0], region.splits[1], region.splits[2], region.splits[3]);
                    if (region.pads != null)
                        ninepatch.SetPads(region.pads[0], region.pads[1], region.pads[2], region.pads[3]);
                    return ninepatch;
                }
            }
            throw new KeyNotFoundException(name);
        }

        public Texture2D GetTexture(int page) => texture[page];

        /// <summary>
        /// Dispose an atlas
        /// </summary>

        public void Dispose()
        {
            foreach(Texture2D page in texture)
            {
                page.Dispose();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>

        public TextureAtlas() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="texture"></param>

        public TextureAtlas(params Texture2D[] texture)
        {
            this.texture = texture;
        }

        /// <summary>
        /// Constructor that is used for textureAtlas creator
        /// </summary>
        /// <param name="regions"></param>

        internal TextureAtlas(List<Region> regions)
        {
            _regions = regions;
        }
    }
}
