using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;

namespace BlueberryCore.Pipeline
{
    [ContentImporter(".atlas", DefaultProcessor = "AtlasProcessor",
    DisplayName = "TextureAtlas importer - BlueberryEngine")]
    public class AtlasImporter : ContentImporter<FileStream>
    {
        public override FileStream Import(string filename, ContentImporterContext context)
        {
            context.Logger.LogMessage("Importing file: {0}", filename);

            return new FileStream(filename, FileMode.Open);
        }
    }
}
