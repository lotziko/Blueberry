
namespace BlueberryCore.Tilemaps
{
    public class DrawableTilemap : Tilemap
    {
        public DrawableTilemap(Tilemap tilemap, TileDictionary dictionary)
        {
            width = tilemap.width;
            height = tilemap.height;
            cellWidth = tilemap.cellWidth;
            cellHeight = tilemap.cellHeight;

            foreach(Tilelayer layer in tilemap.GetTilelayers())
            {
                layer.ReplaceIDsWithTiles(dictionary);
                layers.Add(layer);
            }
        }

    }
}
