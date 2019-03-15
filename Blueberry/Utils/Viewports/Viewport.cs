using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry
{
    public abstract partial class Viewport
    {
        public Camera Camera { get; set; }
        public float WorldWidth { get; set; }
        public float WorldHeight { get; set; }

        public int ScreenX { get; set; }
        public int ScreenY { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }

        private static Vec2 tmp;

        public void SetWorldSize(float width, float height)
        {
            WorldWidth = width;
            WorldHeight = height;
        }

        public void SetScreenSize(int width, int height)
        {
            ScreenWidth = width;
            ScreenHeight = height;
        }

        public void SetScreenBounds(int screenX, int screenY, int screenWidth, int screenHeight)
        {
            ScreenX = screenX;
            ScreenY = screenY;
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
        }

        /** Returns the left gutter (black bar) width in screen coordinates. */
        public int GetLeftGutterWidth()
        {
            return ScreenX;
        }

        /** Returns the right gutter (black bar) x in screen coordinates. */
        public int GetRightGutterX()
        {
            return ScreenX + ScreenWidth;
        }

        /** Returns the right gutter (black bar) width in screen coordinates. */
        public int GetRightGutterWidth()
        {
            return Screen.Width - (ScreenX + ScreenWidth);
        }

        /** Returns the bottom gutter (black bar) height in screen coordinates. */
        public int GetBottomGutterHeight()
        {
            return ScreenY;
        }

        /** Returns the top gutter (black bar) y in screen coordinates. */
        public int GetTopGutterY()
        {
            return ScreenY + ScreenHeight;
        }

        /** Returns the top gutter (black bar) height in screen coordinates. */
        public int GetTopGutterHeight()
        {
            return Screen.Height - (ScreenY + ScreenHeight);
        }

        public void Apply()
        {
            Apply(false);
        }

        public void Apply(bool centerCamera)
        {
            ResetPlatformViewport();
            Camera.SetSize((int)WorldWidth, (int)WorldHeight);
            if (centerCamera)
                Camera.SetPosition(WorldWidth / 2, WorldHeight / 2);
            Camera.ForceCalculate();
        }

        public virtual void Update(int screenWidth, int screenHeight, bool centerCamera)
        {
            Apply(centerCamera);
        }

        public void Update(int screenWidth, int screenHeight)
        {
            Update(screenWidth, screenHeight, false);
        }

        public void Project(ref float worldX, ref float worldY)
        {
            tmp.Set(worldX, worldY);
            tmp = Camera.Unproject(tmp, ScreenX, ScreenY, ScreenWidth, ScreenHeight);
            worldX = tmp.X;
            worldY = tmp.Y;
        }

        public Vec2 Project(Vec2 worldCoords)
        {
            return Camera.Project(worldCoords, ScreenX, ScreenY, ScreenWidth, ScreenHeight);
        }

        public void Unproject(ref float screenX, ref float screenY)
        {
            tmp.Set(screenX, screenY);
            tmp = Camera.Unproject(tmp, ScreenX, ScreenY, ScreenWidth, ScreenHeight);
            screenX = tmp.X;
            screenY = tmp.Y;
        }

        public Vec2 Unproject(Vec2 screenCoords)
        {
            return Camera.Unproject(screenCoords, ScreenX, ScreenY, ScreenWidth, ScreenHeight);
        }

        public Rect CalculateScissors(Mat batchTransform, Rect area)
        {
            return Graphics.CalculateScissors(Camera, ScreenX, ScreenY, ScreenWidth, ScreenHeight, batchTransform, area);
        }
    }
}
