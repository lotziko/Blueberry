using OpenTK;

namespace BlueberryOpenTK
{
    public interface IBatch
    {
        Matrix4 Projection { get; set; }

        Matrix4 Transform { get; set; }

        void Begin();

        void End();

        void Flush();

        void ResetAttributes();
    }
}
