using Blueberry;
using OpenTK;

namespace OpenTKTest
{
    public class MainCore : Blueberry
    {
        public static Blueberry.OpenGL.Core instance;

        public MainCore() : base()
        {
            ForceSceneChange = true;
            instance = this;
            BackgroundColor = new Col(42, 42, 45, 255);
            Scene = new TestScene();
            WindowState = WindowState.Maximized;
        }
    }
}
