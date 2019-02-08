using Microsoft.Xna.Framework;

namespace BlueberryCore
{
    public abstract class BlendMode
    {
        public abstract Color Apply(Color src, Color dst);

        /// <summary>
        /// Do not do anything
        /// </summary>

        public static readonly BlendMode defaultBlending = new DefaultBlendingMode();

        private class DefaultBlendingMode : BlendMode
        {
            public override Color Apply(Color src, Color dst)
            {
                return dst;
            }
        }

        public static readonly BlendMode alphaBlending = new AlphaBlendingMode();

        private class AlphaBlendingMode : BlendMode
        {
            public override Color Apply(Color src, Color dst)
            {
                float srcAlpha = src.A / 255f;
                float dstAlpha = dst.A / 255f;
                //float invAlpha = 1 - alpha;
                return new Color((int)(dst.R * dstAlpha + src.R * (1 - dstAlpha)), (int)(dst.G * dstAlpha + src.G * (1 - dstAlpha)), (int)(dst.B * dstAlpha + src.B * (1 - dstAlpha)), (int)((srcAlpha + (1 - srcAlpha) * dstAlpha) * 255)); ;
            }
        }

        public static readonly BlendMode alphaBlendingPremultiplied = new AlphaBlendingPremultipliedMode();

        private class AlphaBlendingPremultipliedMode : BlendMode
        {
            public override Color Apply(Color src, Color dst)
            {
                float srcAlpha = src.A / 255f;
                float dstAlpha = dst.A / 255f;
                //float invAlpha = 1 - alpha;
                return Color.FromNonPremultiplied((int)(dst.R * dstAlpha + src.R * (1 - dstAlpha)), (int)(dst.G * dstAlpha + src.G * (1 - dstAlpha)), (int)(dst.B * dstAlpha + src.B * (1 - dstAlpha)), (int)((srcAlpha + (1 - srcAlpha) * dstAlpha) * 255));
            }
        }

        public static readonly BlendMode alphaBlendingDstAlpha = new AlphaBlendingDstAlphaMode();

        private class AlphaBlendingDstAlphaMode : BlendMode
        {
            public override Color Apply(Color src, Color dst)
            {
                float srcAlpha = src.A / 255f;
                float dstAlpha = dst.A / 255f;
                //float invAlpha = 1 - alpha;
                return new Color((int)(src.R * srcAlpha + dst.R * (1 - srcAlpha)), (int)(src.G * srcAlpha + dst.G * (1 - srcAlpha)), (int)(src.B * srcAlpha + dst.B * (1 - srcAlpha)), dst.A);
            }
        }

        public static readonly BlendMode eraseAlpha = new EraseAlphaMode();

        private class EraseAlphaMode : BlendMode
        {
            public override Color Apply(Color src, Color dst)
            {
                src.A -= dst.A;
                return src;
            }
        }

        public static readonly BlendMode fade = new FadeMode();

        private class FadeMode : BlendMode
        {
            protected static FloatColor bg, fg, r;
            protected struct FloatColor
            {
                public float R, G, B, A;

                public static FloatColor operator/ (FloatColor color, float value)
                {
                    color.R /= value;
                    color.G /= value;
                    color.B /= value;
                    color.A /= value;
                    return color;
                }
            }

            public override Color Apply(Color src, Color dst)
            {
                src.Deconstruct(out bg.R, out bg.G, out bg.B, out bg.A);
                dst.Deconstruct(out fg.R, out fg.G, out fg.B, out fg.A);
                bg /= 255f;
                fg /= 255f;
                r.A = AlphaFunction();
                if (r.A < 0) return Color.Transparent; // Fully transparent -- R,G,B not important
                r.R = (fg.R - bg.R) * fg.A + bg.R;
                r.G = (fg.G - bg.G) * fg.A + bg.G;
                r.B = (fg.B - bg.B) * fg.A + bg.B;
                //.R = bg.R * fg.R;//(bg.R * bg.A + fg.R * fg.A * (1 - bg.A)) / r.A;
                //r.G = bg.G * fg.G;//(bg.G * bg.A + fg.G * fg.A * (1 - bg.A)) / r.A;
                //r.B = bg.B * fg.B;//(bg.B * bg.A + fg.B * fg.A * (1 - bg.A)) / r.A;
                return new Color(r.R, r.G, r.B, r.A);
            }

            protected virtual float AlphaFunction()
            {
                return 1 - (1 - fg.A) * (1 - bg.A);
            }
        }

        public static readonly BlendMode fadeEraseAlpha = new FadeEraseAlphaMode();

        private class FadeEraseAlphaMode : FadeMode
        {
            protected override float AlphaFunction()
            {
                return fg.A;
            }
        }
    }
}
