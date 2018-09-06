using Newtonsoft.Json;
using System.Collections.Generic;

namespace BlueberryCore.Tilemaps
{
    public class Tilemap
    {
        [JsonProperty]
        internal int width, height, cellWidth, cellHeight;
        [JsonProperty]
        internal List<Tilelayer> layers = new List<Tilelayer>();

        public int GetWidth() => width;

        public int GetHeight() => height;

        public int GetCellWidth() => cellWidth;

        public int GetCellHeight() => cellHeight;

        /// <summary>
        /// Get all layers
        /// </summary>
        /// <returns></returns>

        public List<Tilelayer> GetTilelayers() => layers;

        /// <summary>
        /// Get layer of name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>

        public Tilelayer GetTileLayer(string name)
        {
            foreach (Tilelayer layer in layers)
            {
                if (layer.name == name)
                {
                    return layer;
                }
            }
            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Draw tilemap on coordinates
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>

        public void Draw(Graphics graphics, float x, float y)
        {
            foreach (Tilelayer layer in layers)
            {
                layer.Draw(graphics, x, y);
            }
        }
    }
}
