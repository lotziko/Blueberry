using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace BlueberryCore.Tilemaps
{
    class TilemapReader : ContentTypeReader<TilemapList>
    {
        protected override TilemapList Read(ContentReader input, TilemapList existingInstance)
        {
            var result = new TilemapList();

            var tilemapCount = input.ReadInt32();
            
            for (int i = 0; i < tilemapCount; i++)
            {
                var tilemap = new Tilemap()
                {
                    width = input.ReadInt32(),
                    height = input.ReadInt32(),
                    cellWidth = input.ReadInt32(),
                    cellHeight = input.ReadInt32()
                };

                tilemap.layers = new List<Tilelayer>();

                var layerCount = input.ReadInt32();
                
                for (int j = 0; j < layerCount; j++)
                {
                    var layer = new Tilelayer()
                    {
                        name = input.ReadString(),
                        width = input.ReadInt32(),
                        height = input.ReadInt32(),
                        cellWidth = input.ReadInt32(),
                        cellHeight = input.ReadInt32()
                    };

                    layer.tileIDs = new int[layer.width, layer.height];

                    for (int w = 0; w < layer.width; w++)
                    {
                        for (int h = 0; h < layer.height; h++)
                        {
                            layer.tileIDs[w, h] = input.ReadInt32();
                        }
                    }
                    tilemap.layers.Add(layer);
                }
                result.Add(tilemap);
            }

            return result;
        }
    }
}
