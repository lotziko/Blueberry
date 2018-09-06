using System;

namespace BlueberryCore.SMath
{
    public class SimpleMath
    {
        public static int FloorDiv(float a, float b)
        {
            return (int)Math.Floor(a / b);
        }

        public static int FloorDiv(int a, int b)
        {
            return a / b;
        }

        public static int FloorMod(float a, float b)
        {
            return (int)(a % b);
        }

        public static int FloorMod(int a, int b)
        {
            return a % b;
        }

        public static float Lerp(float num1, float num2, float alpha)
        {
            return num1 + alpha * (num2 - num1);
        }

        public static double Distance(float x1, float y1, float x2, float y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        public static double Direction(float x1, float y1, float x2, float y2)
        {
            return Math.Atan2((y2 - y1), (x2 - x1));
        }

        public static float LengthdirX(float distance, float angle)
        {
            return (float) Math.Cos(Math.PI * angle / 180) * distance;
        }

        public static float LengthdirY(float distance, float angle)
        {
            return (float)Math.Sin(Math.PI * angle / 180) * distance;
        }

        #region Sign

        public static int Sign(int num)
        {
            return num.CompareTo(0f);
        }

        public static int Sign(float num)
        {
            return num.CompareTo(0f);
        }

        #endregion

        #region Clamp

        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            return value;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            return value;
        }

        #endregion

    }
}
