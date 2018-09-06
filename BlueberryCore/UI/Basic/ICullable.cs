using Microsoft.Xna.Framework;

namespace BlueberryCore.UI
{
    public interface ICullable
    {
        void SetCullingArea(Rectangle cullingArea);
    }
}
