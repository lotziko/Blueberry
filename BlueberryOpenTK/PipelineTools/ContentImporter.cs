
using System.IO;

namespace Blueberry.OpenGL.PipelineTools
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
