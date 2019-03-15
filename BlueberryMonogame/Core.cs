using Microsoft.Xna.Framework;
using System;

namespace Blueberry
{
    public partial class Core : IDisposable
    {
        public int Width
        {
            get => window.GraphicsDevice.Viewport.Width;
            set
            {
                var v = window.GraphicsDevice.Viewport;
                v.Width = value;
            }
        }
        public int Height
        {
            get => window.GraphicsDevice.Viewport.Height;
            set
            {
                var v = window.GraphicsDevice.Viewport;
                v.Height = value;
            }
        }

        protected MonogameWindow window;

        public Core()
        {
            window = new MonogameWindow(this);
        }

        public void Run()
        {
            window.Run();
        }

        public void Run(int rate)
        {
            window.IsFixedTimeStep = true;
            window.TargetElapsedTime = TimeSpan.FromSeconds(1.0f / rate);
            window.Run();
        }

        public virtual void RenderFrame()
        {
            graphics.Projection = Screen.DefaultProjection;
            graphics.Transform = Screen.DefaultTransform;
            currentScene?.Render();
            graphics.Flush();
        }

        public virtual void Initialize()
        {
            graphics = new Graphics(window.GraphicsDevice);
            Content.Initialize(window.Content);
        }

        public void Dispose()
        {
            window.Dispose();
        }

        protected class MonogameWindow : Game
        {
            private Core core;

            public GraphicsDeviceManager graphics;

            public MonogameWindow(Core core)
            {
                this.core = core;
                graphics = new GraphicsDeviceManager(this);
                Content.RootDirectory = "Content";
                Window.ClientSizeChanged += OnResize;
            }

            protected override void Update(GameTime gameTime)
            {
                base.Update(gameTime);

                core.ChangeScene();

                if (core.currentScene != null)
                    core.currentScene.Update(gameTime.ElapsedGameTime.Seconds);

                if (Render.NeedsRequest && !Render.Requested)
                {
                    SuppressDraw();
                    return;
                }
            }

            protected override void Draw(GameTime gameTime)
            {
                core.graphics.Clear(core.BackgroundColor);

                if (core.currentScene != null)
                {
                    core.RenderFrame();
                    Render.Complete();
                }
            }

            protected override void Initialize()
            {
                base.Initialize();
                core.Initialize();
            }

            protected void OnResize(object sender, EventArgs e)
            {
                Screen.Width = GraphicsDevice.Viewport.Width;
                Screen.Height = GraphicsDevice.Viewport.Height;
                Render.Request();
                core.currentScene?.OnResize(Screen.Width, Screen.Height);
            }

            protected override void LoadContent()
            {
                base.LoadContent();
                core.Load();
            }
        }
    }
}
