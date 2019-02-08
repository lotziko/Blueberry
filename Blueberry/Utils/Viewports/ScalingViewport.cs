using System;

namespace Blueberry
{
    public class ScalingViewport : Viewport
    {
        public Scaling Scaling { get; set; }

        public ScalingViewport(Scaling scaling, float worldWidth, float worldHeight) : this(scaling, worldWidth, worldHeight, new Camera()) { }

        public ScalingViewport(Scaling scaling, float worldWidth, float worldHeight, Camera camera)
        {
            Scaling = scaling;
            SetWorldSize(worldWidth, worldHeight);
            Camera = camera;
        }

        public override void Update(int screenWidth, int screenHeight, bool centerCamera)
        {
            Vec2 scaled = Scaling.Apply(WorldWidth, WorldHeight, screenWidth, screenHeight);
            int viewportWidth = (int)Math.Round(scaled.X);
            int viewportHeight = (int)Math.Round(scaled.Y);

            SetScreenBounds((screenWidth - viewportWidth) / 2, (screenHeight - viewportHeight) / 2, viewportWidth, viewportHeight);

            Apply(centerCamera);
        }
    }
}
