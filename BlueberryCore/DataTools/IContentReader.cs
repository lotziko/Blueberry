
namespace BlueberryCore.DataTools
{
    interface IContentReader<T>
    {
        T Read(string path);
    }
}
