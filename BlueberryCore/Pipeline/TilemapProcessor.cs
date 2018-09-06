using BlueberryCore.Tilemaps;
using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.Collections.Generic;

namespace BlueberryCore.Pipeline
{
    [ContentProcessor(DisplayName = "Tilemap processor - BlueberryEngine")]
    public class TilemapProcessor : ContentProcessor<String, List<Tilemap>>
    {
        public override List<Tilemap> Process(String input, ContentProcessorContext context)
        {
            try
            {
                context.Logger.LogMessage("Processing");

                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Tilemap>>(input);
            }
            catch (Exception ex)
            {
                context.Logger.LogMessage("Error {0}", ex);
                throw;
            }
        }
    }
}
