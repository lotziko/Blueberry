using Blueberry;
using OpenTK;

namespace OpenTKTest
{
    public class MainCore : BlueberryOpenTK.Core
    {
        public static BlueberryOpenTK.Core instance;

        public MainCore() : base()
        {
            Scene = new TestScene();
            instance = this;
            BackgroundColor = new Col(42, 42, 45, 255);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            graphics.Projection = Screen.DefaultProjection;
            graphics.Transform = Screen.DefaultTransform;

            base.OnRenderFrame(e);

            graphics.Flush();
        }
    }
}
