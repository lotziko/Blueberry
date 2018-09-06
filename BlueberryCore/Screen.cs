using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueberryCore
{
    public static class Screen
    {
        static GraphicsDeviceManager _graphicsDeviceManager;

        public static void Initialize(GraphicsDeviceManager graphicsDeviceManager)
        {
            _graphicsDeviceManager = graphicsDeviceManager;
        }

        public static int Width
        {
            get
            {
                return _graphicsDeviceManager.GraphicsDevice.PresentationParameters.BackBufferWidth;
            }
        }

        public static int Height
        {
            get
            {
                return _graphicsDeviceManager.GraphicsDevice.PresentationParameters.BackBufferHeight;
            }
        }

        public static Rectangle Bounds
        {
            get
            {
                return _graphicsDeviceManager.GraphicsDevice.PresentationParameters.Bounds;
            }
        }
    }
}
