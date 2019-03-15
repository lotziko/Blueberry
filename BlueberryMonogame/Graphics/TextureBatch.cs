using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blueberry.Monogame
{
    public class TextureBatch : IBatch, IDisposable
    {
        const int initialCapacity = 256;
        protected int currentCapacity, pointer, p;
        protected GraphicsDevice device;
        protected Effect shader;
        protected SpriteEffect spriteEffect;
        protected VertexPositionColorTexture[] vertices;
        protected short[] indices;
        protected Texture2D[] textures;
        protected Mat projection, transform;

        public Effect Shader
        {
            get => shader;
            set
            {
                shader = value;
            }
        }

        public Mat Projection
        {
            get => projection;
            set
            {
                projection = value;
            }
        }

        public Mat Transform
        {
            get => transform;
            set
            {
                transform = value;
            }
        }

        public TextureBatch(GraphicsDevice device)
        {
            this.device = device;
            spriteEffect = new SpriteEffect(device);

            device.BlendState = BlendState.AlphaBlend;
            device.SamplerStates[0] = SamplerState.LinearWrap;

            currentCapacity = initialCapacity;
            vertices = new VertexPositionColorTexture[currentCapacity * 4];
            indices = new short[currentCapacity * 6];
            textures = new Texture2D[currentCapacity];

            FillIndices();
        }

        public void Begin()
        {
            Flush();

            spriteEffect.CurrentTechnique.Passes[0].Apply();
        }

        public void End()
        {
            Flush();
        }

        public void Flush()
        {
            if (pointer == 0)
                return;

            Texture2D tex = textures[0];

            int f = 0;
            for(int i = 0; i < pointer; i++)
            {
                if (!ReferenceEquals(tex, textures[i]))
                {
                    FlushVertexArray(f * 4, i * 4, shader, tex);
                    f = i;
                    tex = textures[i];
                }
            }
            if (f < pointer)
            {
                FlushVertexArray(f * 4, pointer * 4, shader, tex);
            }

            pointer = 0;
            p = 0;
        }

        private void FlushVertexArray(int start, int end, Effect effect, Texture2D texture)
        {
            if (start == end)
                return;

            var vertexCount = end - start;

            // If the effect is not null, then apply each pass and render the geometry
            if (effect != null)
            {
                var passes = effect.CurrentTechnique.Passes;
                foreach (var pass in passes)
                {
                    pass.Apply();

                    // Whatever happens in pass.Apply, make sure the texture being drawn
                    // ends up in Textures[0].
                    device.Textures[0] = texture;

                    device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertexCount, indices, start * 6, (vertexCount / 4) * 2, VertexPositionColorTexture.VertexDeclaration);
                }
            }
            else
            {
                // If no custom effect is defined, then simply render.
                device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertexCount, indices, start * 6, (vertexCount / 4) * 2, VertexPositionColorTexture.VertexDeclaration);
            }
        }

        public void Draw(Texture2D texture, float x, float y, float width, float height, float u, float v, float u2, float v2, Color c)
        {
            if ((pointer + 1) * 4 > vertices.Length)
                EnsureArraysCapacity();

            vertices[p].Set(x, y, u, v, c);
            vertices[p + 1].Set(x + width, y, u2, v, c);
            vertices[p + 2].Set(x, y + height, u, v2, c);
            vertices[p + 3].Set(x + width, y + height, u2, v2, c);

            textures[pointer] = texture;
            ++pointer;
            p += 4;
        }

        public void Draw(Texture2D texture, float x, float y, Color c)
        {
            Draw(texture, x, y, texture.Width, texture.Height, c);
        }

        public void Draw(Texture2D texture, float x, float y, float width, float height, Color c)
        {
            if ((pointer + 1) * 4 > vertices.Length)
                EnsureArraysCapacity();

            vertices[p].Set(x, y, 0, 0, c);
            vertices[p + 1].Set(x + width, y, 1, 0, c);
            vertices[p + 2].Set(x, y + height, 0, 1, c);
            vertices[p + 3].Set(x + width, y + height, 1, 1, c);

            textures[pointer] = texture;
            ++pointer;
            p += 4;
        }

        private void FillIndices()
        {
            int p = 0;
            
            for(short i = 0; i < currentCapacity; i++)
            {
                indices[p] = (short)(i * 4);
                indices[p + 1] = (short)(i * 4 + 1);
                indices[p + 2] = (short)(i * 4 + 2);

                indices[p + 3] = (short)(i * 4 + 1);
                indices[p + 4] = (short)(i * 4 + 3);
                indices[p + 5] = (short)(i * 4 + 2);
                p += 6;
            }
        }

        private void EnsureArraysCapacity()
        {
            currentCapacity += initialCapacity;

            var newVertices = new VertexPositionColorTexture[currentCapacity * 4];
            vertices.CopyTo(newVertices, 0);
            vertices = newVertices;

            indices = new short[currentCapacity * 6];

            var newTextures = new Texture2D[currentCapacity];
            textures.CopyTo(newTextures, 0);
            textures = newTextures;

            FillIndices();
        }

        public void Dispose()
        {
            spriteEffect.Dispose();
        }
    }

    internal static class VertexExt
    {
        public static void Set(this ref VertexPositionColorTexture vert, float x, float y, float u, float v, Color c)
        {
            vert.Position.X = x;
            vert.Position.Y = y;
            vert.TextureCoordinate.X = u;
            vert.TextureCoordinate.Y = v;
            vert.Color = c;
        }
    }
}
