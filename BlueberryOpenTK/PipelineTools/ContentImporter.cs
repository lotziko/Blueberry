
using System.IO;

namespace BlueberryOpenTK.PipelineTools
{
    public abstract class ContentImporter<T> : IContentImporter
    {
        public abstract T Import(string filename);

        object IContentImporter.Import(string filename)
        {
            return Import(filename);
        }
    }
}
