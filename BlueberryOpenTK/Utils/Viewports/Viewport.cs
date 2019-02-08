using OpenTK.Graphics.OpenGL4;

namespace Blueberry
{
    public abstract partial class Viewport
    {
        private void ResetPlatformViewport()
        {
            GL.Viewport(ScreenX, ScreenY, ScreenWidth, ScreenHeight);
        }
    }
}
