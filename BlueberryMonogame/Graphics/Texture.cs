using Microsoft.Xna.Framework.Graphics;

namespace Blueberry
{
    public partial class Texture : Texture2D
    {
        public Texture(GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height)
        {
        }

        public Texture(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format) : base(graphicsDevice, width, height, mipmap, format)
        {
        }

        public Texture(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, int arraySize) : base(graphicsDevice, width, height, mipmap, format, arraySize)
        {
        }

        protected Texture(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared, int arraySize) : base(graphicsDevice, width, height, mipmap, format, type, shared, arraySize)
        {
        }
    }
}
