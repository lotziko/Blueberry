using BlueberryCore.Tilemaps;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.Collections.Generic;

namespace BlueberryCore.Pipeline
{
    [ContentTypeWriter]
    class TilemapWriter : ContentTypeWriter<List<Tilemap>>
    {
        protected override void Write(ContentWriter output, List<Tilemap> value)
        {
            output.Write(value.Count);
            
            for(int i = 0; i < value.Count; i++)
            {
                var tilemap = value[i];

                output.Write(tilemap.GetWidth());
                output.Write(tilemap.GetHeight());
                output.Write(tilemap.GetCellWidth());
                output.Write(tilemap.GetCellHeight());

                var layers = tilemap.GetTilelayers();
                output.Write(layers.Count);

                for(int j = 0; j < layers.Count; j++)
                {
                    var layer = layers[j];
                    int width = layer.GetWidth();
                    int height = layer.GetHeight();

                    output.Write(layer.GetName());
                    output.Write(width);
                    output.Write(height);
                    output.Write(layer.GetCellWidth());
                    output.Write(layer.GetCellHeight());

                    var tiles = layer.GetTileIDs();

                    for (int w = 0; w < width; w++)
                    {
                        for(int h = 0; h < height; h++)
                        {
                            output.Write(tiles[w, h]);
                        }
                    }
                }
            }
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(Tilemap).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(TilemapReader).AssemblyQualifiedName;
        }
    }
}
