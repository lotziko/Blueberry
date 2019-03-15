using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace Blueberry.OpenGL
{
    public class TextureBatch : IBatch
    {
        const int initialCapacity = 256;
        protected int currentCapacity, tPtr, vPtr;
        protected GraphicsDevice device;
        protected Effect shader;
        protected SpriteEffect spriteEffect;
        protected VertexPositionColorTexture[] vertices;
        protected ushort[] indices;
        protected Texture[] textures;
        protected Mat transform;

        public Effect Shader
        {
            get => shader;
            set
            {
                shader = value;
            }
        }

        public Mat Transform
        {
            get => transform;
            set
            {
                transform = value;
                spriteEffect.Transform = value.m;
            }
        }

        public TextureBatch(GraphicsDevice device)
        {
            this.device = device;
            spriteEffect = new SpriteEffect(device);

            currentCapacity = initialCapacity;
            vertices = new VertexPositionColorTexture[currentCapacity * 4];
            indices = new ushort[currentCapacity * 6];
            textures = new Texture[currentCapacity];

            FillIndices();
        }

        public void Begin()
        {
            Flush();
        }

        public void End()
        {
            Flush();
        }

        public void Flush()
        {
            if (tPtr == 0)
                return;

            spriteEffect.Apply();

            var tex = textures[0];
            
            device.Textures[0] = tex;
            
            int f = 0;
            for (int i = 0; i < tPtr; i++)
            {
                if (!ReferenceEquals(tex, textures[i]))
                {
                    FlushVertexArray(f, i, shader, tex);
                    f = i;
                    tex = device.Textures[0] = textures[i];
                }
            }

            FlushVertexArray(f, tPtr, shader, tex);
            
            tPtr = 0;
            vPtr = 0;
        }

        private void FlushVertexArray(int start, int end, Effect effect, Texture texture)
        {
            if (start == end)
                return;

            var vertexCount = (end - start) * 4;

            // If the effect is not null, then apply each pass and render the geometry
            if (effect != null)
            {
                effect.Apply();

                device.Textures[0] = texture;

                device.DrawUserIndexedPrimitives(PrimitiveType.Triangles, vertices, 0, vertexCount, indices, start * 6, (vertexCount / 4) * 2, VertexPositionColorTexture.VertexDeclaration);
            }
            else
            {
                // If no custom effect is defined, then simply render.
                device.DrawUserIndexedPrimitives(PrimitiveType.Triangles, vertices, 0, vertexCount, indices, start * 6, (vertexCount / 4) * 2, VertexPositionColorTexture.VertexDeclaration);
            }
        }

        public void Draw(Texture texture, float x, float y, float width, float height, float u, float v, float u2, float v2, Color4 c)
        {
            if ((tPtr + 1) * 4 > vertices.Length)
                EnsureArraysCapacity();

            vertices[vPtr].Set(x, y, u, v, c);
            vertices[vPtr + 1].Set(x + width, y, u2, v, c);
            vertices[vPtr + 2].Set(x, y + height, u, v2, c);
            vertices[vPtr + 3].Set(x + width, y + height, u2, v2, c);

            textures[tPtr] = texture;
            ++tPtr;
            vPtr += 4;
        }

        public void Draw(Texture texture, float x, float y, Color4 c)
        {
            Draw(texture, x, y, texture.Width, texture.Height, c);
        }

        public void Draw(Texture texture, float x, float y, float width, float height, Color4 c)
        {
            if ((tPtr + 1) * 4 > vertices.Length)
                EnsureArraysCapacity();

            vertices[vPtr].Set(x, y, 0, 0, c);
            vertices[vPtr + 1].Set(x + width, y, 1, 0, c);
            vertices[vPtr + 2].Set(x, y + height, 0, 1, c);
            vertices[vPtr + 3].Set(x + width, y + height, 1, 1, c);

            textures[tPtr] = texture;
            ++tPtr;
            vPtr += 4;
        }

        private void FillIndices()
        {
            int p = 0;

            for (short i = 0; i < currentCapacity; i++)
            {
                indices[p] = (ushort)(i * 4);
                indices[p + 1] = (ushort)(i * 4 + 1);
                indices[p + 2] = (ushort)(i * 4 + 2);

                indices[p + 3] = (ushort)(i * 4 + 1);
                indices[p + 4] = (ushort)(i * 4 + 3);
                indices[p + 5] = (ushort)(i * 4 + 2);
                p += 6;
            }
        }

        private void EnsureArraysCapacity()
        {
            currentCapacity += initialCapacity;

            var newVertices = new VertexPositionColorTexture[currentCapacity * 4];
            vertices.CopyTo(newVertices, 0);
            vertices = newVertices;

            indices = new ushort[currentCapacity * 6];

            var newTextures = new Texture[currentCapacity];
            textures.CopyTo(newTextures, 0);
            textures = newTextures;

            FillIndices();
        }

    }
}
