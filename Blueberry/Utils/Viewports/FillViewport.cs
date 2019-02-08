
namespace Blueberry
{
    public class FillViewport : ScalingViewport
    {
        public FillViewport(float worldWidth, float worldHeight) : base(Scaling.fill, worldWidth, worldHeight)
        {
        }

        public FillViewport(float worldWidth, float worldHeight, Camera camera) : base(Scaling.fill, worldWidth, worldHeight, camera)
        {
        }
    }
}
