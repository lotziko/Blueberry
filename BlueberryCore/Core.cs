using BlueberryCore.InputModels;
using BlueberryCore.Properties;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BlueberryCore
{
    public class Core : Game
    {
        public static GraphicsDevice graphicsDevice;

        public static ContentPool content;

        public static BitmapFont font;

        public static GameTime time;

        private Scene _current, _next;

        internal static ContentPool libraryContent;

        internal static PlatformID pid = Environment.OSVersion.Platform;

        internal static bool is64 = Environment.Is64BitProcess;

        GraphicsDeviceManager graphicsManager;

        RenderTarget2D _sceneTarget, _finalTarget;

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

        public static bool renderNeedsRequest = true;
        private static bool renderRequested = true;

        public static void RequestRender()
        {
            renderRequested = true;
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            font = content.Load<BitmapFont>("standardFont");
        }

        public Core(int width = 1280, int height = 720, bool isFullscreen = false, string RootDirectory = "Content")
        {
            graphicsManager = new GraphicsDeviceManager(this);
            graphicsManager.IsFullScreen = isFullscreen;
            graphicsManager.PreferredBackBufferWidth = width;
            graphicsManager.PreferredBackBufferHeight = height;

            Content.RootDirectory = RootDirectory;
            content = new ContentPool(Services, RootDirectory);
            libraryContent = new ContentPool(new ResourceContentManager(Services, Resources.ResourceManager));

            Window.ClientSizeChanged += OnGraphicsDeviceReset;
            IsMouseVisible = true;
            IsFixedTimeStep = true;
            Content.RootDirectory = RootDirectory;
            TargetElapsedTime = TimeSpan.FromSeconds(1d / 30);

            Screen.Initialize(graphicsManager);
        }

        protected override void Initialize()
        {
            base.Initialize();
            graphicsDevice = GraphicsDevice;
            _finalTarget = new RenderTarget2D(graphicsDevice, Screen.Width, Screen.Height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            Graphics.instance = new Graphics(graphicsDevice);
            Input.Initialize(new SDLInputModel());
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            time = gameTime;
            //update global systems here
            TimeUtils.CountFrame();
            Input.Update();

            Window.Title = (GC.GetTotalMemory(false) / 1048576f).ToString("F") + " MB";

            if (_next != null)
            {
                if (_current != null)
                    _current.End();
                _current = _next;
                _next = null;

                _current.Initialize();
                _current.Begin();
            }
            if (_current != null)
                _current.Update();
            if (renderNeedsRequest && !renderRequested)
                SuppressDraw();
        }

        protected override void Draw(GameTime gameTime)
        {
            if (renderNeedsRequest && renderRequested || !renderNeedsRequest)
            {
                graphicsDevice.SetRenderTarget(_finalTarget);
                graphicsDevice.Clear(Color.Black);

                base.Draw(gameTime);

                if (_current != null)
                    _current.Render();
                
                graphicsDevice.SetRenderTarget(null);
            }
            Graphics.instance.Begin();
            Graphics.instance.Draw(_finalTarget, Vector2.Zero);
            Graphics.instance.End();

            renderRequested = false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected void OnGraphicsDeviceReset(object sender, EventArgs e)
        {
            if (_current != null)
                _current.OnGraphicsDeviceReset();
            _finalTarget.Dispose();
            _finalTarget = new RenderTarget2D(graphicsDevice, Screen.Width, Screen.Height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            RequestRender();
        }
    }
}
