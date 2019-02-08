using Blueberry;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;

namespace BlueberryOpenTK
{
    public class Texture2D : IDisposable
    {
        internal int texturePointer;
        protected int width, height;
        protected float texelH, texelV;
        protected Rect bounds;
        protected bool hasMipmap;

        public float TexelH => texelH;
        public float TexelV => texelV;
        public int Width => width;
        public int Height => height;
        public Rect Bounds => bounds;

        public Texture2D(int width, int height, bool hasMipmap = false) : this(width, height, GL.GenTexture(), hasMipmap)
        {
            GL.BindTexture(TextureTarget.Texture2D, texturePointer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            if (hasMipmap)
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public Texture2D(byte[,] data, bool hasMipmap = false) : this(data.GetLength(0), data.GetLength(1), GL.GenTexture(), hasMipmap)
        {
            GL.BindTexture(TextureTarget.Texture2D, texturePointer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            if (hasMipmap)
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public Texture2D(byte[] data, int width, int height, bool hasMipmap = false) : this(width, height, GL.GenTexture(), hasMipmap)
        {
            GL.BindTexture(TextureTarget.Texture2D, texturePointer);
            //GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            //GL.PixelStore(PixelStoreParameter.UnpackAlignment, 8);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            if (hasMipmap)
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        /*public Texture2D(Bitmap bitmap, bool hasMipmap = false) : this(bitmap.Width, bitmap.Height, GL.GenTexture(), hasMipmap)
        {
            var bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.BindTexture(TextureTarget.Texture2D, texturePointer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);

            bitmap.UnlockBits(bmpData);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            if (hasMipmap)
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }*/

        private Texture2D(int width, int height, int id, bool hasMipmap)
        {
            this.width = width;
            this.height = height;
            this.hasMipmap = hasMipmap;
            texturePointer = id;
            bounds.Width = width;
            bounds.Height = height;
            texelH = 1.0f / width;
            texelV = 1.0f / height;
        }

        public void SetData(byte[] data, Rect bounds, PixelFormat format, PixelType type)
        {
            int previous = GL.GetInteger(GetPName.TextureBinding2D);
            GL.BindTexture(TextureTarget.Texture2D, texturePointer);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, (int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height, format, type, data);
            GL.BindTexture(TextureTarget.Texture2D, previous);
        }

        public void SetData(byte[] data, int x, int y, int width, int height, PixelFormat format, PixelType type)
        {
            int previous = GL.GetInteger(GetPName.TextureBinding2D);
            GL.BindTexture(TextureTarget.Texture2D, texturePointer);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, width, height, format, type, data);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 8);
            GL.BindTexture(TextureTarget.Texture2D, previous);

            var pixels = new int[Width * Height];
            GL.BindTexture(TextureTarget.Texture2D, texturePointer);
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            GL.BindTexture(TextureTarget.Texture2D, previous);
        }

        public static Texture2D LoadFromFile(string path)
        {
            var data = Blueberry.DataTools.Image.Load(path);

            int width = data.Width, height = data.Height;

            int tId = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, tId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.bitmap);
            
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            return new Texture2D(width, height, tId, false);
        }

        public void Dispose()
        {
            //GL.DeleteTexture(texturePointer);
        }
    }
}
