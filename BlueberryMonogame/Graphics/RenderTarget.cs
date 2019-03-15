using Microsoft.Xna.Framework.Graphics;

namespace Blueberry
{
    public partial class RenderTarget : RenderTarget2D
    {
        public RenderTarget(GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height)
        {
        }

        public RenderTarget(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat) : base(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat)
        {
        }

        public RenderTarget(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage) : base(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage)
        {
        }

        public RenderTarget(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared) : base(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, shared)
        {
        }

        public RenderTarget(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared, int arraySize) : base(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, shared, arraySize)
        {
        }

        protected RenderTarget(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat format, DepthFormat depthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, SurfaceType surfaceType) : base(graphicsDevice, width, height, mipMap, format, depthFormat, preferredMultiSampleCount, usage, surfaceType)
        {
        }
    }
}
