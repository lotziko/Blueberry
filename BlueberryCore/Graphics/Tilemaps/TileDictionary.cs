using BlueberryCore.TextureAtlases;
using System;
using System.Collections.Generic;

namespace BlueberryCore.Tilemaps
{
    public class TileDictionary : Dictionary<int, Tile>
    {
        public TileDictionary(TextureAtlas atlas)
        {
            var textureRegions = atlas.FindRegions();
            foreach (KeyValuePair<String, TextureRegion> entry in textureRegions)
            {
                var key = entry.Key;
                var slashPos = key.IndexOf('/');
                var tileset = key.Substring(0, slashPos);
                var id = int.Parse(key.Substring(slashPos + 1, key.Length - slashPos - 1));
                
                Add(id, new Tile(id, entry.Value, tileset));
            }
        }

        public List<Tile> GetAll(String name)
        {
            var result = new List<Tile>();
            foreach(KeyValuePair < int, Tile > entry in this)
            {
                if (entry.Value.GetTileset().Equals(name))
                {
                    result.Add(entry.Value);
                }
            }
            return result;
        }
    }
}
