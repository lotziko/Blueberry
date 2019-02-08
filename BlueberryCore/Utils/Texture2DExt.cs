using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlueberryCore
{
    public static class Texture2DExt
    {
        private static Rectangle pixelBounds = new Rectangle(0, 0, 1, 1);
        private static Color[] pixelData = new Color[1];

        public static void SetPixel(this Texture2D texture, int x, int y, Color color)
        {
            if (x >= 0 && x < texture.Width && y >= 0 && y <= texture.Height)
            {
                pixelBounds.X = x;
                pixelBounds.Y = y;
                pixelData[0] = color;
                texture.SetData(0, pixelBounds, pixelData, 0, 1);
            }
        }

        public static Color GetPixel(this Texture2D texture, int x, int y)
        {
            if (x >= 0 && x < texture.Width && y >= 0 && y <= texture.Height)
            {
                pixelBounds.X = x;
                pixelBounds.Y = y;
                texture.GetData(0, pixelBounds, pixelData, 0, 1);
                return pixelData[0].FromPremultiplied();
            }
            return default;
        }
    }
}
