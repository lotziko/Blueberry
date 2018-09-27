using BlueberryCore.SMath;
using Microsoft.Xna.Framework;
using System;

namespace BlueberryCore
{
    public static class ColorExt
    {
        /// <summary>
        /// Converts hsv to rgb
        /// </summary>
        /// <param name="color"></param>
        /// <param name="h">from 0 to 359</param>
        /// <param name="S">from 0 to 100</param>
        /// <param name="V">from 0 to 100</param>
        /*public static void FromHSV(this ref Color color, float h, float S, float V)
        {
            S /= 100f;
            V /= 100f;

            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            color.R = (byte)SimpleMath.Clamp((int)(R * 255.0), 0, 255);
            color.G = (byte)SimpleMath.Clamp((int)(G * 255.0), 0, 255);
            color.B = (byte)SimpleMath.Clamp((int)(B * 255.0), 0, 255);
        }*/
        public static void FromHSV(this ref Color color, float h, float s, float v, byte a = 255)
        {
            s /= 100;
            v /= 100;

            double r = 0, g = 0, b = 0;

            if (s == 0)
            {
                r = v;
                g = v;
                b = v;
            }
            else
            {
                int i;
                double f, p, q, t;

                if (h == 360)
                    h = 0;
                else
                    h = h / 60;

                i = (int)Math.Truncate(h);
                f = h - i;

                p = v * (1.0 - s);
                q = v * (1.0 - (s * f));
                t = v * (1.0 - (s * (1.0 - f)));

                switch (i)
                {
                    case 0:
                        r = v;
                        g = t;
                        b = p;
                        break;

                    case 1:
                        r = q;
                        g = v;
                        b = p;
                        break;

                    case 2:
                        r = p;
                        g = v;
                        b = t;
                        break;

                    case 3:
                        r = p;
                        g = q;
                        b = v;
                        break;

                    case 4:
                        r = t;
                        g = p;
                        b = v;
                        break;

                    default:
                        r = v;
                        g = p;
                        b = q;
                        break;
                }

            }
            color.R = (byte)SimpleMath.Clamp((int)(r * 255.0), 0, 255);
            color.G = (byte)SimpleMath.Clamp((int)(g * 255.0), 0, 255);
            color.B = (byte)SimpleMath.Clamp((int)(b * 255.0), 0, 255);
            color.A = a;
        }
        
        public static Color CreateFromHSV(this Color color, float h, float S, float V, byte a = 255)
        {
            color.FromHSV(h, S, V, a);
            return color;
        }

        public static Vector3 ToHSV(this Color rgb)
        {
            float r = rgb.R / 255f;
            float g = rgb.G / 255f;
            float b = rgb.B / 255f;
            float h, s, v;
            float min, max, delta;

            min = Math.Min(Math.Min(r, g), b);
            max = Math.Max(Math.Max(r, g), b);
            v = max;

            delta = max - min;

            if (max != 0)
                s = delta / max;
            else
            {
                s = 0;
                h = 0;
                return new Vector3(MathF.Round(h), MathF.Round(s), MathF.Round(v));
            }

            if (delta == 0)
                h = 0;
            else
            {

                if (r == max)
                    h = (g - b) / delta;
                else if (g == max)
                    h = 2 + (b - r) / delta;
                else
                    h = 4 + (r - g) / delta;
            }

            h *= 60;
            if (h < 0)
                h += 360;

            s *= 100;
            v *= 100;

            return new Vector3(MathF.Round(h), MathF.Round(s), MathF.Round(v));
        }

        public static Vector3 ToHSVOne(this Color rgb)
        {
            float r = rgb.R / 255f;
            float g = rgb.G / 255f;
            float b = rgb.B / 255f;
            float h, s, v;
            float min, max, delta;

            min = Math.Min(Math.Min(r, g), b);
            max = Math.Max(Math.Max(r, g), b);
            v = max;

            delta = max - min;

            if (max != 0)
                s = delta / max;
            else
            {
                s = 0;
                h = 0;
                return new Vector3(h, s, v / 100);
            }

            if (delta == 0)
                h = 0;
            else
            {

                if (r == max)
                    h = (g - b) / delta;
                else if (g == max)
                    h = 2 + (b - r) / delta;
                else
                    h = 4 + (r - g) / delta;
            }

            h *= 60;
            if (h < 0)
                h += 360;
            
            return new Vector3(h / 360, s, v);
        }

        public static Color FromPremultiplied(this Color color)
        {
            if (color.A == 0)
                return color;
            byte A = color.A;
            float a = 1.0f / ((float)A / 255);
            color *= a;
            color.A = A;
            return color;
        }
    }
}
