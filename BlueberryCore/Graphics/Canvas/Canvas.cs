using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BlueberryCore
{
    public class Canvas
    {
        protected Texture2D texture;
        protected Color[] bitmap;
        protected BlendMode mode;
        protected bool textureIsDirty;

        public Texture2D CurrentTexture
        {
            get
            {
                if (textureIsDirty)
                {
                    texture.SetData(bitmap);
                    textureIsDirty = false;
                }
                return texture;
            }
            set
            {
                texture = value;
                bitmap = new Color[texture.Width * texture.Height];
                texture.GetData(bitmap);
            }
        }

        public Canvas(BlendMode mode, Texture2D texture)
        {
            this.mode = mode;
            CurrentTexture = texture;
        }

        #region Drawing

        //Place pixels on bitmap only and convert it to texture when needed

        public void Clear(Color color)
        {
            for (int i = 0; i < bitmap.Length; i++)
                bitmap[i] = color;
            textureIsDirty = true;
        }

        #region Point

        public void DrawPoint(int x, int y, Color color)
        {
            if (x >= 0 && x < texture.Width && y >= 0 && y < texture.Height)
                bitmap[x + y * texture.Width] = color;
        }

        public void DrawPoint(float x, float y, Color color)
        {
            if (x >= 0 && x < texture.Width && y >= 0 && y < texture.Height)
                bitmap[(int)Math.Round(x) + (int)Math.Round(y) * texture.Width] = color;
        }

        public Color GetPoint(float x, float y)
        {
            if (x >= 0 && x < texture.Width && y >= 0 && y < texture.Height)
                return bitmap[Convert.ToInt32(x) + Convert.ToInt32(y) * texture.Width];
            return default;
        }

        public bool TestColor(Color b, Color a, int tolerance)
        {
            int sum = 0;
            int diff;

            diff = a.R - b.R;
            sum += (1 + diff * diff) * a.A / 255;

            diff = a.G - b.G;
            sum += (1 + diff * diff) * a.A / 255;

            diff = a.B - b.B;
            sum += (1 + diff * diff) * a.A / 255;

            diff = a.A - b.A;
            sum += diff * diff;

            return (sum <= tolerance * tolerance * 4);
        }

        #endregion

        #region Pen

        protected void DrawPen(float x, float y, Pen pen)
        {
            var map = pen.Bitmap;
            int offsetX = pen.OffsetX, offsetY = pen.OffsetY;
            for(int i = 0; i < pen.Width; i++)
                for(int j = 0; j < pen.Height; j++)
                    DrawPoint(x - offsetX + i, y - offsetY + j, map[j * pen.Width + i]);
        }

        #endregion

        #region Line

        public void DrawLine(float x1, float y1, float x2, float y2, Pen pen)
        {
            if (Math.Abs(y2 - y1) < Math.Abs(x2 - x1))
            {
                if (x1 > x2)
                    DrawLineLow(x2, y2, x1, y1, pen);
                else
                    DrawLineLow(x1, y1, x2, y2, pen);
            }
            else
            {
                if (y1 > y2)
                    DrawLineHigh(x2, y2, x1, y1, pen);
                else
                    DrawLineHigh(x1, y1, x2, y2, pen);
            }
            textureIsDirty = true;
        }

        protected void DrawLineLow(float x1, float y1, float x2, float y2, Pen pen)
        {
            var dx = x2 - x1;
            var dy = y2 - y1;
            var yi = 1;
            if (dy < 0)
            {
                yi = -1;
                dy = -dy;
            }
            var D = 2 * dy - dx;
            var y = y1;
            
            for(float x = x1; x <= x2; x++)
            {
                DrawPen(x, y, pen);
                if (D > 0)
                {
                    y += yi;
                    D -= 2 * dx;
                }
                D += 2 * dy;
            }
        }

        protected void DrawLineHigh(float x1, float y1, float x2, float y2, Pen pen)
        {
            var dx = x2 - x1;
            var dy = y2 - y1;
            var xi = 1;
            if (dx < 0)
            {
                xi = -1;
                dx = -dx;
            }
            var D = 2 * dx - dy;
            var x = x1;
            
            for (float y = y1; y <= y2; y++)
            {
                DrawPen(x, y, pen);
                if (D > 0)
                {
                    x += xi;
                    D -= 2 * dy;
                }
                D += 2 * dx;
            }
        }

        #endregion

        #region Rectangle

        public void DrawRectangle(int x, int y, int width, int height, Color color)
        {
            DrawTriangle(x, y, x + width, y, x, y + height, color);
            DrawTriangle(x + width, y, x + width, y + height, x, y + height, color);
            textureIsDirty = true;
        }

        #endregion

        #region Triangle

        public void DrawTriangle(float x1, float y1, float x2, float y2, float x3, float y3, Color color)
        {
            float left = SimpleMath.Min(x1, x2, x3);
            float right = SimpleMath.Max(x1, x2, x3);
            float top = SimpleMath.Min(y1, y2, y3);
            float bottom = SimpleMath.Max(y1, y2, y3);
            
            float px, py;
            for(float i = left; i < right; i++)
            {
                for(float j = top; j < bottom; j++)
                {
                    px = i + 0.5f;
                    py = j + 0.5f;
                    
                    if (PointInTriangle(x1, y1, x2, y2, x3, y3, px, py))
                    {
                        DrawPoint(i, j, color);
                        //posInBitmap = Convert.ToInt32(i) + Convert.ToInt32(j) * texture.Width;
                        //bitmap[posInBitmap] = color;//mode.Apply(bitmap[posInBitmap], color);
                    }
                }
            }

            textureIsDirty = true;
        }

        #endregion

        #region Circle

        public void DrawCircle(float xc, float yc, float radius, Pen pen)
        {
            float x = 0;
            float y = radius;
            float d = (5 - radius * 4) / 4;
            do
            {
                DrawPen(xc + x, yc + y, pen);
                DrawPen(xc + y, yc + x, pen);
                DrawPen(xc - y, yc + x, pen);
                DrawPen(xc - x, yc + y, pen);
                DrawPen(xc - x, yc - y, pen);
                DrawPen(xc - y, yc - x, pen);
                DrawPen(xc + y, yc - x, pen);
                DrawPen(xc + x, yc - y, pen);

                if (d < 0)
                {
                    d += 2 * x + 1;
                }
                else
                {
                    d += 2 * (x - y) + 1;
                    y--;
                }
                x++;
            }
            while (x <= y);
            textureIsDirty = true;
        }

        #endregion

        #region Ellipse

        public void DrawEllipse(float xc, float yc, float width, float height, Pen pen)
        {
            if (width == height)
            {
                DrawCircle(xc, yc, width, pen);
                return;
            }
            float a2 = width * width;
            float b2 = height * height;
            float fa2 = 4 * a2, fb2 = 4 * b2;
            float x, y, sigma;

            /* first half */
            for (x = 0, y = height, sigma = 2 * b2 + a2 * (1 - 2 * height); b2 * x <= a2 * y; x++)
            {
                DrawPen(xc + x, yc + y, pen);
                DrawPen(xc - x, yc + y, pen);
                DrawPen(xc + x, yc - y, pen);
                DrawPen(xc - x, yc - y, pen);
                if (sigma >= 0)
                {
                    sigma += fa2 * (1 - y);
                    y--;
                }
                sigma += b2 * ((4 * x) + 6);
            }

            /* second half */
            for (x = width, y = 0, sigma = 2 * a2 + b2 * (1 - 2 * width); a2 * y <= b2 * x; y++)
            {
                DrawPen(xc + x, yc + y, pen);
                DrawPen(xc - x, yc + y, pen);
                DrawPen(xc + x, yc - y, pen);
                DrawPen(xc - x, yc - y, pen);
                if (sigma >= 0)
                {
                    sigma += fb2 * (1 - x);
                    x--;
                }
                sigma += a2 * ((4 * y) + 6);
            }
            textureIsDirty = true;
        }

        #endregion

        #region Fill

        public void Fill(int x, int y, Color color, float tolerance)
        {
            int tol = (int)(tolerance * tolerance * 256);
            var targetColor = GetPoint(x, y);
            if (targetColor == color)
                return;

            var pixels = new Stack<Point>();
            
            pixels.Push(new Point(x, y));
            while (pixels.Count != 0)
            {
                var temp = pixels.Pop();
                int y1 = temp.Y;
                while (y1 >= 0 && TestColor(GetPoint(temp.X, y1), targetColor, tol))
                {
                    y1--;
                }
                y1++;
                bool spanLeft = false;
                bool spanRight = false;
                while (y1 < texture.Height && TestColor(GetPoint(temp.X, y1), targetColor, tol))
                {
                    DrawPoint(temp.X, y1, color);

                    if (/*!spanLeft &&*/ temp.X > 0 && TestColor(GetPoint(temp.X - 1, y1), targetColor, tol))
                    {
                        pixels.Push(new Point(temp.X - 1, y1));
                        spanLeft = true;
                    }
                    else if (spanLeft && temp.X - 1 == 0 && !TestColor(GetPoint(temp.X - 1, y1), targetColor, tol))
                    {
                        spanLeft = false;
                    }
                    if (/*!spanRight &&*/ temp.X < texture.Width - 1 && TestColor(GetPoint(temp.X + 1, y1), targetColor, tol))
                    {
                        pixels.Push(new Point(temp.X + 1, y1));
                        spanRight = true;
                    }
                    else if (spanRight && temp.X < texture.Width - 1 && !TestColor(GetPoint(temp.X + 1, y1), targetColor, tol))
                    {
                        spanRight = false;
                    }
                    y1++;
                }   
            }
            textureIsDirty = true;
        }

        #endregion

        #region Low level drawing

        private bool EdgeFunction(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            return ((x1 - x3) * (y2 - y3) - (x2 - x3) * (y1 - y3) < 0);
        }

        private bool PointInTriangle(float x1, float y1, float x2, float y2, float x3, float y3, float px, float py)
        {
            bool b1, b2, b3;
            b1 = EdgeFunction(px, py, x1, y1, x2, y2);
            b2 = EdgeFunction(px, py, x2, y2, x3, y3);
            b3 = EdgeFunction(px, py, x3, y3, x1, y1);

            return ((b1 == b2) && (b2 == b3));
        }

        #endregion

        #endregion
    }
}
