using BlueberryCore.TextureAtlases;
using Newtonsoft.Json;
using System;

namespace BlueberryCore.Tilemaps
{
    public class Tile
    {
        [JsonProperty]
        internal int ID;
        [JsonProperty]
        internal TextureRegion image;
        [JsonProperty]
        internal String tileset;

        public String GetTileset() => tileset;
        public int GetID() => ID;
        public TextureRegion GetImage() => image;

        public void Draw(Graphics graphics, float x, float y)
        {
            image.Draw(graphics, x, y);
        }

        public Tile(int ID, TextureRegion image)
        {
            this.ID = ID;
            this.image = image;
        }

        public Tile(int ID, TextureRegion image, String tileset)
        {
            this.ID = ID;
            this.image = image;
            this.tileset = tileset;
        }

    }
}
