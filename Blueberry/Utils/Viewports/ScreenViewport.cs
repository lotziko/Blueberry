
namespace Blueberry
{
    public class ScreenViewport : Viewport
    {
        public float UnitsPerPixel = 1f;

        public ScreenViewport() : this(new Camera()) { }

        public ScreenViewport(Camera camera)
        {
            Camera = camera;
        }

        public override void Update(int screenWidth, int screenHeight, bool centerCamera)
        {
            SetScreenBounds(0, 0, screenWidth, screenHeight);
            SetWorldSize(screenWidth * UnitsPerPixel, screenHeight * UnitsPerPixel);
            Apply(centerCamera);
        }
    }
}
