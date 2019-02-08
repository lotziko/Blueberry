using Blueberry;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;

namespace BlueberryOpenTK
{
    public class TextureBatch : IBatch
    {
        const int initialCapacity = 256;
        protected int vbo, vao, currentCapacity, pointer;
        protected bool isDirty = false;
        protected PolygonMode currentMode;

        protected Vertex[] vertices;
        protected int[] textures;

        private ShaderProgram shader = ShaderProgram.BasicTexture;
        private Matrix4 projection, transform;

        public PolygonMode Mode
        {
            get => currentMode;
            set
            {
                if (value != currentMode)
                {
                    Flush();
                    currentMode = value;
                    GL.PolygonMode(MaterialFace.FrontAndBack, value);
                }
            }
        }

        public ShaderProgram Shader
        {
            get => shader;
            set
            {
                if ((shader = value) == null)
                    shader = ShaderProgram.BasicTexture;
                GL.ActiveTexture(TextureUnit.Texture0);
                shader.Params["tex"].SetValue(0);
                //ResetAttributes();
            }
        }

        public Matrix4 Projection
        {
            get => projection;
            set
            {
                projection = value;
                shader.Params["projection"].SetValue(projection);
            }
        }

        public Matrix4 Transform
        {
            get => transform;
            set
            {
                transform = value;
                shader.Params["transform"].SetValue(transform);
            }
        }


        public TextureBatch()
        {
            currentCapacity = initialCapacity;
            vertices = new Vertex[currentCapacity * 4];
            textures = new int[currentCapacity];

            GL.GenVertexArrays(1, out vao);

            GL.GenBuffers(1, out vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * 8 * 4, vertices, BufferUsageHint.StreamDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            Projection = Matrix4.Identity;
            Transform = Matrix4.Identity;

            ResetAttributes();
        }

        public void ResetAttributes()
        {
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            GL.VertexAttribPointer(shader.positionAttribute, 2, VertexAttribPointerType.Float, false, 8 * 4, 0);
            GL.EnableVertexAttribArray(shader.positionAttribute);

            GL.VertexAttribPointer(shader.textureCoordAttribute, 2, VertexAttribPointerType.Float, false, 8 * 4, 2 * 4);
            GL.EnableVertexAttribArray(shader.textureCoordAttribute);

            GL.VertexAttribPointer(shader.colorAttribute, 4, VertexAttribPointerType.Float, false, 8 * 4, 4 * 4);
            GL.EnableVertexAttribArray(shader.colorAttribute);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
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
            if (pointer == 0)
                return;

            GL.UseProgram(shader.Program);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BindVertexArray(vao);

            if (isDirty)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * 8 * 4, vertices, BufferUsageHint.StreamDraw);
                isDirty = false;
            }
            else
            {
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, pointer * 8 * 4 * 4, vertices);
            }

            GL.BindTexture(TextureTarget.Texture2D, textures[0]);
            int f = 0;
            for(int i = 0, tex = textures[0]; i < pointer; i++)
            {
                if (textures[i] != tex)
                {
                    GL.DrawArrays(PrimitiveType.Quads, f * 4, (i - f) * 4);
                    f = i;
                    tex = textures[i];
                    GL.BindTexture(TextureTarget.Texture2D, tex);
                }
            }
            if (f < pointer)
            {
                GL.DrawArrays(PrimitiveType.Quads, f * 4, (pointer - f) * 4);
            }

            GL.UseProgram(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);

            pointer = 0;
        }

        public void AddTexture(Texture2D texture, float x, float y)
        {

        }

        public void AddTexture(Texture2D texture, float x, float y, Color4 color)
        {
            if ((pointer + 1) * 4 > vertices.Length)
                EnsureArraysCapacity();

            int p = pointer * 4;

            vertices[p].Set(x, y, 0, 0, color);
            vertices[p + 1].Set(x + texture.Width, y, 1, 0, color);
            vertices[p + 2].Set(x + texture.Width, y + texture.Height, 1, 1, color);
            vertices[p + 3].Set(x, y + texture.Height, 0, 1, color);

            textures[pointer] = texture.texturePointer;
            ++pointer;
        }

        public void AddTexture(Texture2D texture, Rect destination, Color4 color)
        {
            if ((pointer + 1) * 4 > vertices.Length)
                EnsureArraysCapacity();

            int p = pointer * 4;

            vertices[p].Set(destination.X, destination.Y, 0, 0, color);
            vertices[p + 1].Set(destination.Right, destination.Y, 1, 0, color);
            vertices[p + 2].Set(destination.Right, destination.Bottom, 1, 1, color);
            vertices[p + 3].Set(destination.X, destination.Bottom, 0, 1, color);

            textures[pointer] = texture.texturePointer;
            ++pointer;
        }

        public void AddTexture(Texture2D texture, Rect source, Rect destination, Color4 color)
        {
            if ((pointer + 1) * 4 > vertices.Length)
                EnsureArraysCapacity();

            int p = pointer * 4;

            vertices[p].Set(destination.Left, destination.Top, source.X * texture.TexelH, source.Y * texture.TexelV, color);
            vertices[p + 1].Set(destination.Right, destination.Top, source.Right * texture.TexelH, source.Top * texture.TexelV, color);
            vertices[p + 2].Set(destination.Right, destination.Bottom, source.Right * texture.TexelH, source.Bottom * texture.TexelV, color);
            vertices[p + 3].Set(destination.Left, destination.Bottom, source.Left * texture.TexelH, source.Bottom * texture.TexelV, color);

            textures[pointer] = texture.texturePointer;
            ++pointer;
        }

        private void EnsureArraysCapacity()
        {
            currentCapacity += initialCapacity;

            var newVertices = new Vertex[currentCapacity * 4];
            vertices.CopyTo(newVertices, 0);
            vertices = newVertices;

            var newTextures = new int[currentCapacity];
            textures.CopyTo(newTextures, 0);
            textures = newTextures;

            isDirty = true;
        }


        protected struct Vertex
        {
            float x, y, u, v, r, g, b, a;

            public void Set(float x, float y, float u, float v, Color4 c)
            {
                this.x = x;
                this.y = y;
                this.u = u;
                this.v = v;
                r = c.R;
                g = c.G;
                b = c.B;
                a = c.A;
            }
        }
    }
}
