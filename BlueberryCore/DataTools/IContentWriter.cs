
namespace BlueberryCore.DataTools
{
    interface IContentWriter<T>
    {
        void Write(string path, T data);
    }
}
