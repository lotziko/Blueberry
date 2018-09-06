using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.IO;

namespace BlueberryCore.Pipeline
{
    [ContentImporter(".tms", DefaultProcessor = "TilemapProcessor",
    DisplayName = "Tilemap importer - BlueberryEngine")]
    public class TilemapImporter : ContentImporter<String>
    {
        public override String Import(string filename, ContentImporterContext context)
        {
            context.Logger.LogMessage("Importing file: {0}", filename);

            return File.ReadAllText(filename);
        }
    }
}
