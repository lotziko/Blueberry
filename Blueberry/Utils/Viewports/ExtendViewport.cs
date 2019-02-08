
using System;

namespace Blueberry
{
    public class ExtendViewport : Viewport
    {
        public float MinWorldWidth { get; set; }
        public float MinWorldHeight { get; set; }
        public float MaxWorldWidth { get; set; }
        public float MaxWorldHeight { get; set; }

        public ExtendViewport(float minWorldWidth, float minWorldHeight) : this(minWorldWidth, minWorldHeight, 0, 0, new Camera())
        {
        }

        public ExtendViewport(float minWorldWidth, float minWorldHeight, Camera camera) : this(minWorldWidth, minWorldHeight, 0, 0, camera)
        {
        }

        public ExtendViewport(float minWorldWidth, float minWorldHeight, float maxWorldWidth, float maxWorldHeight) : this(minWorldWidth, minWorldHeight, maxWorldWidth, maxWorldHeight, new Camera())
        {
        }

        public ExtendViewport(float minWorldWidth, float minWorldHeight, float maxWorldWidth, float maxWorldHeight, Camera camera)
        {
            MinWorldWidth = minWorldWidth;
            MinWorldHeight = minWorldHeight;
            MaxWorldWidth = maxWorldWidth;
            MaxWorldHeight = maxWorldHeight;
            Camera = camera;
        }

        public override void Update(int screenWidth, int screenHeight, bool centerCamera)
        {
            // Fit min size to the screen.
            float worldWidth = MinWorldWidth;
            float worldHeight = MinWorldHeight;
            Vec2 scaled = Scaling.fit.Apply(worldWidth, worldHeight, screenWidth, screenHeight);

            // Extend in the short direction.
            int viewportWidth = (int)Math.Round(scaled.X);
            int viewportHeight = (int)Math.Round(scaled.Y);
            if (viewportWidth < screenWidth)
            {
                float toViewportSpace = viewportHeight / worldHeight;
                float toWorldSpace = worldHeight / viewportHeight;
                float lengthen = (screenWidth - viewportWidth) * toWorldSpace;
                if (MaxWorldWidth > 0) lengthen = Math.Min(lengthen, MaxWorldWidth - MinWorldWidth);
                worldWidth += lengthen;
                viewportWidth += (int)Math.Round(lengthen * toViewportSpace);
            }
            else if (viewportHeight < screenHeight)
            {
                float toViewportSpace = viewportWidth / worldWidth;
                float toWorldSpace = worldWidth / viewportWidth;
                float lengthen = (screenHeight - viewportHeight) * toWorldSpace;
                if (MaxWorldHeight > 0) lengthen = Math.Min(lengthen, MaxWorldHeight - MinWorldHeight);
                worldHeight += lengthen;
                viewportHeight += (int)Math.Round(lengthen * toViewportSpace);
            }

            SetWorldSize(worldWidth, worldHeight);

            // Center.
            SetScreenBounds((screenWidth - viewportWidth) / 2, (screenHeight - viewportHeight) / 2, viewportWidth, viewportHeight);

            Apply(centerCamera);
        }
    }
}
