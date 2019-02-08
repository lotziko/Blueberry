using Blueberry;
using BlueberryOpenTK;
using OpenTK;
using OpenTK.Input;

namespace OpenTKTest
{
    public class MainCore : BlueberryOpenTK.Core
    {
        private RenderTarget2D target;

        public MainCore() : base()
        {
            Scene = new TestScene();

            target = new RenderTarget2D(600, 400);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            graphics.Projection = Screen.DefaultProjection;
            graphics.Transform = Screen.DefaultTransform;

            //graphics.SetRenderTarget(target);
            base.OnRenderFrame(e);
            //graphics.ResetRenderTarget();

            //graphics.DrawTexture(target, 0, 0);
            graphics.Flush();
        }
    }
}
