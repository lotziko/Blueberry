using System;
using System.Collections.Generic;

namespace Blueberry
{
    public partial class TextureAtlas
    {
        /// <summary>
        /// Holds texture regions accessed by name
        /// </summary>

        internal List<AtlasRegion> regions = new List<AtlasRegion>();
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

        internal TextureAtlas(List<AtlasRegion> regions)
        {
            this.regions = regions;
        }

        /// <summary>
        /// Get first region by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>

        public AtlasRegion FindRegion(string name)
        {
            for (var i = 0; i < Count; i++)
            {
                if (regions[i].name == name)
                {
                    return regions[i];
                }
            }
            throw new KeyNotFoundException(name);
        }

        /// <summary>
        /// Get all regions of name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>

        public List<AtlasRegion> FindRegions(string name)
        {
            var result = new List<AtlasRegion>();
            for (var i = 0; i < Count; i++)
            {
                if (regions[i].name == name)
                {
                    result.Add(regions[i]);
                }
            }
            return result;
        }

        /// <summary>
        /// Get all regions
        /// </summary>
        /// <returns></returns>

        public Dictionary<string, AtlasRegion> FindRegions()
        {
            var result = new Dictionary<string, AtlasRegion>();
            for (var i = 0; i < Count; i++)
            {
                var region = regions[i];
                result.Add(region.name, region);
            }
            return result;
        }

        public List<AtlasRegion> Regions => regions;

        public NinePatch FindNinePatch(string name)
        {
            for (var i = 0; i < Count; i++)
            {
                if (regions[i].name == name)
                {
                    var region = regions[i];
                    if (region.splits == null)
                        throw new Exception("TextureRegion is not a ninepatch");
                    var ninepatch = new NinePatch(region, region.splits[0], region.splits[1], region.splits[2], region.splits[3]);
                    if (region.pads != null)
                        ninepatch.SetPads(region.pads[0], region.pads[1], region.pads[2], region.pads[3]);
                    return ninepatch;
                }
            }
            throw new KeyNotFoundException(name);
        }
    }
}
