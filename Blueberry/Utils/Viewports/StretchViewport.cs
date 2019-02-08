using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry
{
    public class StretchViewport : ScalingViewport
    {
        public StretchViewport(float worldWidth, float worldHeight) : base(Scaling.stretch, worldWidth, worldHeight)
        {
        }

        public StretchViewport(float worldWidth, float worldHeight, Camera camera) : base(Scaling.stretch, worldWidth, worldHeight, camera)
        {
        }
    }
}
