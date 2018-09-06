using System;

namespace BlueberryCore
{
    public abstract class Interpolation
    {
        /// <param name="a">Alpha value between 0 and 1.</param>
        /// <returns></returns>
        public abstract float Apply(float a);

        /// <param name="a">Alpha value between 0 and 1.</param>
        /// <returns></returns>
        public float Apply(float start, float end, float a)
        {
            return start + (end - start) * Apply(a);
        }

        public static readonly Interpolation linear = new LinearInterpolation();

        private class LinearInterpolation : Interpolation
        {
            public override float Apply(float a)
            {
                return a;
            }
        }

        public static readonly Interpolation smooth = new SmoothInterpolation();

        private class SmoothInterpolation : Interpolation
        {
            public override float Apply(float a)
            {
                return a * a * (3 - 2 * a);
            }
        }

        public static readonly Interpolation smooth2 = new Smooth2Interpolation();

        private class Smooth2Interpolation : Interpolation
        {
            public override float Apply(float a)
            {
                a = a * a * (3 - 2 * a);
                return a * a * (3 - 2 * a);
            }
        }

        public static readonly Interpolation smoother = new SmootherInterpolation();

        private class SmootherInterpolation : Interpolation
        {
            public override float Apply(float a)
            {
                return a * a * a * (a * (a * 6 - 15) + 10);
            }
        }
        
        public static readonly Interpolation fade = smoother;



        public static readonly Pow pow2 = new Pow(2);
        //slow then fast
        public static readonly PowIn pow2In = new PowIn(2);
        public static readonly PowIn slowFast = pow2In;
        //fast then slow
        public static readonly PowOut pow2Out = new PowOut(2);
        public static readonly PowOut fastSlow = pow2Out;

        
        public static readonly Interpolation pow2InInverse = new Pow2InInverseInterpolation();

        private class Pow2InInverseInterpolation : Interpolation
        {
            public override float Apply(float a)
            {
                return (float)Math.Sqrt(a);
            }
        }

        public static readonly Interpolation pow2OutInverse = new Pow2OutInverseInterpolation();

        private class Pow2OutInverseInterpolation : Interpolation
        {
            public override float Apply(float a)
            {
                return 1 - (float)Math.Sqrt(-(a - 1));
            }
        }




        public static readonly Pow pow3 = new Pow(3);
        public static readonly PowIn pow3In = new PowIn(3);
        public static readonly PowOut pow3Out = new PowOut(3);

        public static readonly Interpolation pow3InInverse = new Pow3InInverseInterpolation();

        private class Pow3InInverseInterpolation : Interpolation
        {
            public override float Apply(float a)
            {
                return (float)Math.Pow(a, 1d / 3);
            }
        }

        public static readonly Interpolation pow3OutInverse = new Pow3OutInverseInterpolation();

        private class Pow3OutInverseInterpolation : Interpolation
        {
            public override float Apply(float a)
            {
                return 1 - (float)Math.Pow(-(a - 1), 1d / 3);
            }
        }



        public static readonly Pow pow4 = new Pow(4);
        public static readonly PowIn pow4In = new PowIn(4);
        public static readonly PowOut pow4Out = new PowOut(4);

        public static readonly Pow pow5 = new Pow(5);
        public static readonly PowIn pow5In = new PowIn(5);
        public static readonly PowOut pow5Out = new PowOut(5);



        public static Interpolation sine = new SineInterpolation();

        private class SineInterpolation : Interpolation
        {
            public override float Apply(float a)
            {
                return (float)((1 - Math.Cos(a * Math.PI)) / 2);
            }
        }

        public static Interpolation sineIn = new SineInInterpolation();

        private class SineInInterpolation : Interpolation
        {
            public override float Apply(float a)
            {
                return 1 - (float) Math.Cos(a * Math.PI / 2);
            }
        }

        public static Interpolation sineOut = new SineOutInterpolation();

        private class SineOutInterpolation : Interpolation
        {
            public override float Apply(float a)
            {
                return (float) Math.Sin(a * Math.PI / 2);
            }
        }



        public static readonly Exp exp10 = new Exp(2, 10);
        public static readonly ExpIn exp10In = new ExpIn(2, 10);
        public static readonly ExpOut exp10Out = new ExpOut(2, 10);

        public static readonly Exp exp5 = new Exp(2, 5);
        public static readonly ExpIn exp5In = new ExpIn(2, 5);
        public static readonly ExpOut exp5Out = new ExpOut(2, 5);
        

        public static readonly Interpolation circle = new CircleInterpolation();

        private class CircleInterpolation : Interpolation
        {
            public override float Apply(float a)
            {
                if (a <= 0.5f)
                {
                    a *= 2;
                    return (1 - (float)Math.Sqrt(1 - a * a)) / 2;
                }
                a--;
                a *= 2;
                return ((float)Math.Sqrt(1 - a * a) + 1) / 2;
            }
        }

        public static readonly Interpolation circleIn = new CircleInInterpolation();

        private class CircleInInterpolation : Interpolation
        {
            public override float Apply(float a)
            {
                return 1 - (float)Math.Sqrt(1 - a * a);
            }
        }

        public static readonly Interpolation circleOut = new CircleOutInterpolation();

        private class CircleOutInterpolation : Interpolation
        {
            public override float Apply(float a)
            {
                a--;
                return (float)Math.Sqrt(1 - a * a);
            }
        }


        public static readonly Elastic elastic = new Elastic(2, 10, 7, 1);
        public static readonly ElasticIn elasticIn = new ElasticIn(2, 10, 6, 1);
        public static readonly ElasticOut elasticOut = new ElasticOut(2, 10, 7, 1);

        public static readonly Swing swing = new Swing(1.5f);
        public static readonly SwingIn swingIn = new SwingIn(2f);
        public static readonly SwingOut swingOut = new SwingOut(2f);

        public static readonly Bounce bounce = new Bounce(4);
        public static readonly BounceIn bounceIn = new BounceIn(4);
        public static readonly BounceOut bounceOut = new BounceOut(4);

        #region Pow

        public class Pow : Interpolation
        {
            protected readonly int power;

            public Pow(int power)
            {
                this.power = power;
            }

            public override float Apply(float a)
            {
                if (a <= 0.5f) return (float)Math.Pow(a * 2, power) / 2;
                return (float)Math.Pow((a - 1) * 2, power) / (power % 2 == 0 ? -2 : 2) + 1;
            }
        }

        public class PowIn : Pow
        {
            public PowIn(int power) : base(power) { }

            public override float Apply(float a)
            {
                return (float)Math.Pow(a, power);
            }
        }

        public class PowOut : Pow
        {
            public PowOut(int power) : base(power) { }

            public override float Apply(float a)
            {
                return (float)Math.Pow(a - 1, power) * (power % 2 == 0 ? -1 : 1) + 1;
            }
        }

        #endregion

        #region Exp

        public class Exp : Interpolation
        {
            protected readonly float value, power, min, scale;

            public Exp(float value, float power)
            {
                this.value = value;
                this.power = power;
                min = (float)Math.Pow(value, -power);
                scale = 1 / (1 - min);
            }

            public override float Apply(float a)
            {
                if (a <= 0.5f) return ((float)Math.Pow(value, power * (a * 2 - 1)) - min) * scale / 2;
                return (2 - ((float)Math.Pow(value, -power * (a * 2 - 1)) - min) * scale) / 2;
            }
        }

        public class ExpIn : Exp
        {
            public ExpIn(float value, float power) : base(value, power) { }

            public override float Apply(float a)
            {
                return ((float)Math.Pow(value, power * (a - 1)) - min) * scale;
            }
        }

        public class ExpOut : Exp
        {
            public ExpOut(float value, float power) : base(value, power) { }

            public override float Apply(float a)
            {
                return 1 - ((float)Math.Pow(value, -power * a) - min) * scale;
            }
        }

        #endregion

        #region Elastic

        public class Elastic : Interpolation
        {
            protected readonly float value, power, scale, bounces;

            public Elastic(float value, float power, int bounces, float scale)
            {
                this.value = value;
                this.power = power;
                this.scale = scale;
                this.bounces = (float)(bounces * Math.PI * (bounces % 2 == 0 ? 1 : -1));
            }

            public override float Apply(float a)
            {
                if (a <= 0.5f)
                {
                    a *= 2;
                    return (float)(Math.Pow(value, power * (a - 1)) * Math.Sin(a * bounces) * scale / 2);
                }
                a = 1 - a;
                a *= 2;
                return 1 - (float)(Math.Pow(value, power * (a - 1)) * Math.Sin((a) * bounces) * scale / 2);
            }
        }

        public class ElasticIn : Elastic
        {
            public ElasticIn(float value, float power, int bounces, float scale) : base(value, power, bounces, scale) { }

            public override float Apply(float a)
            {
                if (a >= 0.99) return 1;
                return (float)(Math.Pow(value, power * (a - 1)) * Math.Sin(a * bounces) * scale);
            }
        }

        public class ElasticOut : Elastic
        {
            public ElasticOut(float value, float power, int bounces, float scale) : base(value, power, bounces, scale) { }

            public override float Apply(float a)
            {
                if (a == 0) return 0;
                a = 1 - a;
                return (1 - (float)(Math.Pow(value, power * (a - 1)) * Math.Sin(a * bounces) * scale));
            }
        }

        #endregion

        #region Bounce

        public class Bounce : BounceOut
        {
            public Bounce(int bounces) : base(bounces) { }

            public Bounce(float[] widths, float[] heights) : base(widths, heights) { }

            private float Out(float a)
            {
                float test = a + widths[0] / 2;
                if (test < widths[0]) return test / (widths[0] / 2) - 1;
                return base.Apply(a);
            }

            public override float Apply(float a)
            {
                if (a <= 0.5f) return (1 - Out(1 - a * 2)) / 2;
                return Out(a * 2 - 1) / 2 + 0.5f;
            }
        }

        public class BounceOut : Interpolation
        {
            protected readonly float[] widths, heights;

            public BounceOut(float[] widths, float[] heights)
            {
                if (widths.Length != heights.Length)
                    throw new ArgumentException("Must be the same number of widths and heights.");
                this.widths = widths;
                this.heights = heights;
            }

            public BounceOut(int bounces)
            {
                if (bounces < 2 || bounces > 5) throw new ArgumentException("bounces cannot be < 2 or > 5: " + bounces);
                widths = new float[bounces];
                heights = new float[bounces];
                heights[0] = 1;
                switch (bounces)
                {
                    case 2:
                        widths[0] = 0.6f;
                        widths[1] = 0.4f;
                        heights[1] = 0.33f;
                        break;
                    case 3:
                        widths[0] = 0.4f;
                        widths[1] = 0.4f;
                        widths[2] = 0.2f;
                        heights[1] = 0.33f;
                        heights[2] = 0.1f;
                        break;
                    case 4:
                        widths[0] = 0.34f;
                        widths[1] = 0.34f;
                        widths[2] = 0.2f;
                        widths[3] = 0.15f;
                        heights[1] = 0.26f;
                        heights[2] = 0.11f;
                        heights[3] = 0.03f;
                        break;
                    case 5:
                        widths[0] = 0.3f;
                        widths[1] = 0.3f;
                        widths[2] = 0.2f;
                        widths[3] = 0.1f;
                        widths[4] = 0.1f;
                        heights[1] = 0.45f;
                        heights[2] = 0.3f;
                        heights[3] = 0.15f;
                        heights[4] = 0.06f;
                        break;
                }
                widths[0] *= 2;
            }

            public override float Apply(float a)
            {
                if (a == 1) return 1;
                a += widths[0] / 2;
                float width = 0, height = 0;
                for (int i = 0, n = widths.Length; i < n; i++)
                {
                    width = widths[i];
                    if (a <= width)
                    {
                        height = heights[i];
                        break;
                    }
                    a -= width;
                }
                a /= width;
                float z = 4 / width * height * a;
                return 1 - (z - z * a) * width;
            }
        }

        public class BounceIn : BounceOut
        {
            public BounceIn(int bounces) : base(bounces) { }

            public BounceIn(float[] widths, float[] heights) : base(widths, heights) { }

            public override float Apply(float a)
            {
                return 1 - base.Apply(1 - a);
            }
        }

        #endregion

        #region Swing

        public class Swing : Interpolation
        {
            private readonly float scale;

            public Swing(float scale)
            {
                this.scale = scale * 2;
            }

            public override float Apply(float a)
            {
                if (a <= 0.5f)
                {
                    a *= 2;
                    return a * a * ((scale + 1) * a - scale) / 2;
                }
                a--;
                a *= 2;
                return a * a * ((scale + 1) * a + scale) / 2 + 1;
            }
        }

        public class SwingOut : Interpolation
        {
            private readonly float scale;

            public SwingOut(float scale)
            {
                this.scale = scale * 2;
            }

            public override float Apply(float a)
            {
                a--;
                return a * a * ((scale + 1) * a + scale) + 1;
            }
        }

        public class SwingIn : Interpolation
        {
            private readonly float scale;

            public SwingIn(float scale)
            {
                this.scale = scale * 2;
            }

            public override float Apply(float a)
            {
                return a * a * ((scale + 1) * a - scale);
            }
        }

        #endregion

    }
}
