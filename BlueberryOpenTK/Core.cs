using Blueberry;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Threading;

namespace Blueberry
{
    public partial class Core
    {
        public readonly Graphics graphics;
        public Col BackgroundColor { get; set; } = Col.Black;
        public bool ForceSceneChange { get; set; } = false;

        public Scene Scene
        {
            get
            {
                return currentScene;
            }
            set
            {
                nextScene = value;
                if (ForceSceneChange)
                    ChangeScene();
            }
        }

        public int Width { get => window.Width; set => window.Width = value; }
        public int Height { get => window.Height; set => window.Height = value; }

        protected Scene currentScene, nextScene;
        protected OpenTKWindow window;

        public Core()
        {
            window = new OpenTKWindow(this)
            {
                VSync = VSyncMode.Adaptive
            };
            Input.Initialize(window);

            graphics = new Graphics();
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
            graphics.Projection = Screen.DefaultProjection;
            graphics.Transform = Screen.DefaultTransform;
            currentScene?.Render();
            graphics.Flush();
        }

        public void Run(int updateRate, int renderRate)
        {
            window.SleepTime = 1000 / renderRate;
            window.Run(updateRate, renderRate);
        }

        protected void ChangeScene()
        {
            if (nextScene != null)
            {
                if (currentScene != null)
                    currentScene.End();
                currentScene = nextScene;
                nextScene = null;

                currentScene.graphics = graphics;
                currentScene.Initialize();
                currentScene.OnResize(Width, Height);
                currentScene.Begin();
            }
        }

        protected class OpenTKWindow : GameWindow
        {
            private Core core;

            public int SleepTime { get; set; }

            public OpenTKWindow(Core core)
            {
                this.core = core;
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
                Thread.Sleep(16);
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                GL.Viewport(0, 0, Width, Height);
                Screen.Width = Width;
                Screen.Height = Height;
                Render.Request();
                core.currentScene?.OnResize(Width, Height);
            }
        }
    }
}
