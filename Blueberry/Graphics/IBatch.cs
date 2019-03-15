
namespace Blueberry
{
    public interface IBatch
    {
        Mat Transform { get; set; }

        void Begin();

        void End();

        void Flush();
    }
}
