using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlueberryCore
{
    public class Pen
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int OffsetX { get { return Width / 2; } }
        public int OffsetY { get { return Height / 2; } }
        public Color[] Bitmap { get; protected set; }

        protected Texture2D texture;

        public Texture2D Texture
        {
            get
            {
                if (texture == null)
                {
                    texture = new Texture2D(Core.graphicsDevice, Width, Height);
                    texture.SetData(Bitmap);
                }
                return texture;
            }
        }

        public Color Color { get; set; } = Color.White;

        public Pen()
        {
            Bitmap = new Color[] { Color.White };
            Width = 1;
            Height = 1;
        }

        public Pen(Microsoft.Xna.Framework.Graphics.Texture2D texture)
        {
            Bitmap = new Color[texture.Width * texture.Height];
            texture.GetData(Bitmap);
            Width = texture.Width;
            Height = texture.Height;
        }

        public Pen(TextureRegion region)
        {
            var source = region.GetSource();
            Bitmap = new Color[source.Width * source.Height];
            region.GetTexture().GetData(0, source, Bitmap, 0, Bitmap.Length);
            Width = source.Width;
            Height = source.Height;
        }
    }
}
