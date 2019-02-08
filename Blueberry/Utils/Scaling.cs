
namespace Blueberry
{
    public enum Scaling
    {
        /** Scales the source to fit the target while keeping the same aspect ratio. This may cause the source to be smaller than the
         * target in one direction. */
        fit,
        /** Scales the source to fill the target while keeping the same aspect ratio. This may cause the source to be larger than the
         * target in one direction. */
        fill,
        /** Scales the source to fill the target in the x direction while keeping the same aspect ratio. This may cause the source to be
         * smaller or larger than the target in the y direction. */
        fillX,
        /** Scales the source to fill the target in the y direction while keeping the same aspect ratio. This may cause the source to be
         * smaller or larger than the target in the x direction. */
        fillY,
        /** Scales the source to fill the target. This may cause the source to not keep the same aspect ratio. */
        stretch,
        /** Scales the source to fill the target in the x direction, without changing the y direction. This may cause the source to not
         * keep the same aspect ratio. */
        stretchX,
        /** Scales the source to fill the target in the y direction, without changing the x direction. This may cause the source to not
         * keep the same aspect ratio. */
        stretchY,
        /** The source is not scaled. */
        none
    }

    public static class ScalingExt
    {
        private static Vec2 temp;

        public static Vec2 Apply(this Scaling scaling, float sourceWidth, float sourceHeight, float targetWidth, float targetHeight)
        {
            float targetRatio, sourceRatio, scale;
            switch (scaling)
            {
                case Scaling.fit:
                    targetRatio = targetHeight / targetWidth;
                    sourceRatio = sourceHeight / sourceWidth;
                    scale = targetRatio > sourceRatio ? targetWidth / sourceWidth : targetHeight / sourceHeight;
                    temp.X = sourceWidth * scale;
                    temp.Y = sourceHeight * scale;
                    break;
                case Scaling.fill:
                    targetRatio = targetHeight / targetWidth;
                    sourceRatio = sourceHeight / sourceWidth;
                    scale = targetRatio < sourceRatio ? targetWidth / sourceWidth : targetHeight / sourceHeight;
                    temp.X = sourceWidth * scale;
                    temp.Y = sourceHeight * scale;
                    break;
                case Scaling.fillX:
                    scale = targetWidth / sourceWidth;
                    temp.X = sourceWidth * scale;
                    temp.Y = sourceHeight * scale;
                    break;
                case Scaling.fillY:
                    scale = targetHeight / sourceHeight;
                    temp.X = sourceWidth * scale;
                    temp.Y = sourceHeight * scale;
                    break;
                case Scaling.stretch:
                    temp.X = targetWidth;
                    temp.Y = targetHeight;
                    break;
                case Scaling.stretchX:
                    temp.X = targetWidth;
                    temp.Y = sourceHeight;
                    break;
                case Scaling.stretchY:
                    temp.X = sourceWidth;
                    temp.Y = targetHeight;
                    break;
                case Scaling.none:
                    temp.X = sourceWidth;
                    temp.Y = sourceHeight;
                    break;
            }
            return temp;
        }
    }
}
