
namespace Blueberry.DataTools
{
    public abstract class ContentExporter<T> : IContentExporter
    {
        public abstract void Export(string output, string name, T data);

        void IContentExporter.Export(string output, string name, object data)
        {
            if (data is T)
                Export(output, name, (T)data);
        }
    }
}
