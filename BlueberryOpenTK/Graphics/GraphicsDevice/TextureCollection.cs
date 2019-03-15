using OpenTK.Graphics.OpenGL4;

namespace Blueberry.OpenGL
{
    public sealed class TextureCollection
    {
        private readonly Texture[] textures;
        private readonly GraphicsDevice device;
        private bool dirty = true;

        internal TextureCollection(GraphicsDevice graphicsDevice, int maxTextures)
        {
            device = graphicsDevice;
            textures = new Texture[maxTextures];
        }

        public Texture this[int index]
        {
            get => textures[index];
            set
            {
                if (textures[index] == value)
                    return;

                textures[index] = value;
                dirty = true;
            }
        }

        public void UpdateTextures()
        {
            if (!dirty)
                return;

            for (var i = 0; i < textures.Length; i++)
            {
                var tex = textures[i];

                GL.ActiveTexture(TextureUnit.Texture0 + i);
                
                if (tex != null)
                {
                    GL.BindTexture(TextureTarget.Texture2D, tex.texturePointer);
                }
                else
                {
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                }
            }
        }
    }
}
