using Blueberry.OpenGL;
using OpenTK;
using System;
using System.Threading;

namespace Blueberry
{
    public partial class Core : IDisposable
    {
        public int Width { get => window.Width; set => window.Width = value; }
        public int Height { get => window.Height; set => window.Height = value; }

        protected OpenTKWindow window;

        public Core()
        {
            window = new OpenTKWindow(this)
            {
                VSync = VSyncMode.Adaptive
            };
            Input.Initialize(window);

            graphics = new Graphics(window.GraphicsDevice);
            Screen.Width = Width;
            Screen.Height = Height;
            Render.NeedsRequest = true;
        }

        public void Run(int rate)
        {
            window.SleepTime = 1000 / rate;
            window.Run(rate);
        }

        public virtual void RenderFrame()
        {
            //graphics.Projection = Screen.DefaultProjection;
            graphics.Transform = Screen.DefaultTransform;
            currentScene?.Render();
            graphics.Flush();
        }

        public void Run(int updateRate, int renderRate)
        {
            window.SleepTime = 1000 / renderRate;
            window.Run(updateRate, renderRate);
        }

        public void Dispose()
        {
            window.Dispose();
        }

        protected class OpenTKWindow : GameWindow
        {
            private Core core;

            public int SleepTime { get; set; }
            public GraphicsDevice GraphicsDevice { get; }

            public OpenTKWindow(Core core)
            {
                this.core = core;
                GraphicsDevice = new GraphicsDevice();
            }

            protected override void OnUpdateFrame(FrameEventArgs e)
            {
                base.OnUpdateFrame(e);

                core.ChangeScene();

                if (core.currentScene != null)
                    core.currentScene.Update((float)e.Time);
            }

            protected override void OnRenderFrame(FrameEventArgs e)
            {
                base.OnRenderFrame(e);

                if (Render.NeedsRequest && !Render.Requested)
                {
                    Thread.Sleep(SleepTime);
                    return;
                }

                core.graphics.Clear(core.BackgroundColor);

                if (core.currentScene != null)
                {
                    core.RenderFrame();
                    Render.Complete();
                }

                SwapBuffers();
                Thread.Sleep(SleepTime);
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                Screen.Width = Width;
                Screen.Height = Height;
                GraphicsDevice.Viewport = new Rect(0, 0, Width, Height);
                core.currentScene?.OnResize(Width, Height);
                Render.Request();
            }

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);
                core.Load();
            }
        }
    }
}
