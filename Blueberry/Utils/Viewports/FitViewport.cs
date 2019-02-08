
namespace Blueberry
{
    public class FitViewport : ScalingViewport
    {
        public FitViewport(float worldWidth, float worldHeight) : base(Scaling.fit, worldWidth, worldHeight)
        {
        }

        public FitViewport(float worldWidth, float worldHeight, Camera camera) : base(Scaling.fit, worldWidth, worldHeight, camera)
        {
        }
    }
}
