using Blueberry;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Threading;

namespace BlueberryOpenTK
{
    public class Core : GameWindow
    {
        public readonly Graphics graphics;

        private Scene _current, _next;

        public Col BackgroundColor { get; set; } = Col.Black;
        
        public Scene Scene
        {
            get
            {
                return _current;
            }
            set
            {
                _next = value;
            }
        }

        public Core()
        {
            graphics = new Graphics();
            Screen.Width = Width;
            Screen.Height = Height;
            Render.NeedsRequest = true;
            Input.Initialize(this);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (_next != null)
            {
                if (_current != null)
                    _current.End();
                _current = _next;
                _next = null;

                _current.graphics = graphics;
                _current.Initialize();
                _current.OnResize(Width, Height);
                _current.Begin();
            }
            if (_current != null)
                _current.Update((float)e.Time);
        }
        
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            if (Render.NeedsRequest && !Render.Requested)
            {
                Thread.Sleep(16);
                return;
            }

            graphics.Clear(BackgroundColor);
            
            if (_current != null)
            {
                _current.Render();
                Render.Complete();
            }

            SwapBuffers();
            Thread.Sleep(16);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
            Screen.Width = Width;
            Screen.Height = Height;
            Render.Request();
            _current?.OnResize(Width, Height);
        }
    }
}
