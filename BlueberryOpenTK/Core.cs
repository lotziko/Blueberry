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

        public event EventHandler<KeyEventArgs> KeyDown;
        public event EventHandler<KeyEventArgs> KeyUp;
        public event EventHandler<KeyPressEventArgs> KeyPress;
        public event EventHandler<MouseButtonEventArgs> MouseUp;
        public event EventHandler<MouseButtonEventArgs> MouseDown;
        public event EventHandler<MouseMoveEventArgs> MouseMove;
        public event EventHandler<ScrollEventArgs> MouseWheel;

        protected Scene currentScene, nextScene;
        protected OpenTKWindow window;

        public Core()
        {
            window = new OpenTKWindow(this)
            {
                VSync = VSyncMode.Adaptive
            };

            graphics = new Graphics();
            Screen.Width = Width;
            Screen.Height = Height;
            Render.NeedsRequest = true;
            Input.Initialize(this);
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

            protected override void OnKeyUp(KeyboardKeyEventArgs e)
            {
                core.KeyUp?.Invoke(this, new KeyEventArgs((Key)e.Key, e.Shift, e.Control, e.Alt));
            }

            protected override void OnKeyDown(KeyboardKeyEventArgs e)
            {
                core.KeyDown?.Invoke(this, new KeyEventArgs((Key)e.Key, e.Shift, e.Control, e.Alt));
            }

            protected override void OnKeyPress(OpenTK.KeyPressEventArgs e)
            {
                core.KeyPress?.Invoke(this, new KeyPressEventArgs(e.KeyChar));
            }

            protected override void OnMouseDown(OpenTK.Input.MouseButtonEventArgs e)
            {
                core.MouseDown?.Invoke(this, new MouseButtonEventArgs((MouseButton)e.Button, e.X, e.Y));
            }

            protected override void OnMouseUp(OpenTK.Input.MouseButtonEventArgs e)
            {
                core.MouseUp?.Invoke(this, new MouseButtonEventArgs((MouseButton)e.Button, e.X, e.Y));
            }

            protected override void OnMouseMove(OpenTK.Input.MouseMoveEventArgs e)
            {
                core.MouseMove?.Invoke(this, new MouseMoveEventArgs(e.X, e.Y));
            }

            protected override void OnMouseWheel(MouseWheelEventArgs e)
            {
                core.MouseWheel?.Invoke(this, new ScrollEventArgs(e.Mouse.Scroll.X, e.Mouse.Scroll.Y));
            }
        }
    }
}
