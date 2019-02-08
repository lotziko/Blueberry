using System;
using System.Collections.Generic;

namespace Blueberry
{
    public partial class TextureAtlas
    {
        /// <summary>
        /// Holds texture regions accessed by name
        /// </summary>

        internal List<Region> regions = new List<Region>();
        internal string assetName;
        internal int Count => regions.Count;

        /// <summary>
        /// Constructor
        /// </summary>

        public TextureAtlas() { }

        /// <summary>
        /// Constructor that is used for textureAtlas creator
        /// </summary>
        /// <param name="regions"></param>

        internal TextureAtlas(List<Region> regions)
        {
            this.regions = regions;
        }

        /// <summary>
        /// Get first region by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>

        public TextureRegion FindRegion(string name)
        {
            for (var i = 0; i < Count; i++)
            {
                if (regions[i].name == name)
                {
                    var region = regions[i];
                    return CreateRegion(region.page, region.left, region.top, region.width, region.height, region.offsetX, region.offsetY);
                }
            }
            throw new KeyNotFoundException(name);
        }

        /// <summary>
        /// Get all regions of name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>

        public List<TextureRegion> FindRegions(string name)
        {
            var result = new List<TextureRegion>();
            for (var i = 0; i < Count; i++)
            {
                if (regions[i].name == name)
                {
                    var region = regions[i];
                    result.Add(CreateRegion(region.page, region.left, region.top, region.originalWidth, region.originalHeight, region.offsetX, region.offsetY));
                }
            }
            return result;
        }

        /// <summary>
        /// Get all regions
        /// </summary>
        /// <returns></returns>

        public Dictionary<string, TextureRegion> FindRegions()
        {
            var result = new Dictionary<string, TextureRegion>();
            for (var i = 0; i < Count; i++)
            {
                var region = regions[i];
                result.Add(regions[i].name, CreateRegion(region.page, region.left, region.top, region.originalWidth, region.originalHeight, region.offsetX, region.offsetY));
            }
            return result;
        }

        public NinePatch FindNinePatch(string name)
        {
            for (var i = 0; i < Count; i++)
            {
                if (regions[i].name == name)
                {
                    var region = regions[i];
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
    }

    internal class Region
    {
        internal string name;
        internal int index = -1, left, top, width, height, originalWidth, originalHeight, offsetX, offsetY, page;
        internal int[] splits, pads;
        internal bool rotate;

        /// <summary>
        /// Get name
        /// </summary>
        /// <returns></returns>

        public string Name => name;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>

        public Region(string name, int left, int top, int width, int height)
        {
            this.name = name;
            this.left = left;
            this.top = top;
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Constructor
        /// </summary>

        public Region() { }

        public override string ToString()
        {
            return name;
        }
    }
}
