using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace Blueberry.DataTools
{
    public static class Image
    {
        public static ImageData Load(string path)
        {
            using (var img = System.Drawing.Image.FromFile(path))
            {
                using (var bmp = new Bitmap(img))
                {
                    var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                    var result = new byte[data.Stride * bmp.Height];
                    Marshal.Copy(data.Scan0, result, 0, result.Length);
                    bmp.UnlockBits(data);
                    return new ImageData(result, data.Width, data.Height);
                };
            };
        }

        public static void Save(ImageData data, string path)
        {
            var bmp = new Bitmap(data.Width, data.Height);
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var ptr = bmpData.Scan0;
            for(int i = 0; i < data.Height; i++)
            {
                Marshal.Copy(data.bitmap, i * bmpData.Stride, ptr, bmpData.Stride);
                ptr += bmpData.Stride;
            }
            bmp.UnlockBits(bmpData);
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            bmp.Dispose();
        }
    }

    public class ImageData
    {
        public byte[] bitmap;
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        private const int bpp = 4;

        public ImageData(byte[] bitmap, int width, int height)
        {
            this.bitmap = bitmap;
            Width = width;
            Height = height;
        }

        public void Crop(int left, int right, int top, int bottom)
        {
            int newWidth = Width - left - right, newHeight = Height - top - bottom;
            var newBitmap = new byte[newWidth * newHeight * bpp];

            for(int i = left, n = Width - right; i < n; i++)
            {
                for(int j = top, nn = Height - bottom; j < nn; j++)
                {
                    Array.Copy(bitmap, (i + j * Width) * bpp, newBitmap, ((i - left) + (j - top) * newWidth) * bpp, bpp);
                }
            }

            bitmap = newBitmap;
            Width = newWidth;
            Height = newHeight;
        }

        public void GetPixel(int x, int y, ref byte[] color)
        {
            if (bitmap == null)
                throw new Exception("No image data");
            if (x >= Width || x < 0)
                throw new Exception("x must be >= 0 and < width");
            if (y >= Height || y < 0)
                throw new Exception("y must be >= 0 and < height");

            Array.Copy(bitmap, (x + y * Width) * bpp, color, 0, bpp);
        }
    }
}
