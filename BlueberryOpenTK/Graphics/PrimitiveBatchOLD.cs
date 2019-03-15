using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;

namespace Blueberry.OpenGL
{
    public class PrimitiveBatchOLD : IBatch
    {
        const int initialCapacity = 256;
        protected int vbo, vao, currentCapacity, pointer;
        protected bool isDirty = false;

        protected Vertex[] vertices;

        //private Effect shader = Effect.BasicPrimitive;
        private Mat projection, transform;

        public Effect Shader
        {
            get => null;
            set
            {
                //if ((shader = value) == null)
                  //  shader = Effect.BasicPrimitive;
                //ResetAttributes();
            }
        }

        public Mat Projection
        {
            get => projection;
            set
            {
                projection = value;
                //shader.Uniforms["projection"].SetValue(projection.m);
            }
        }

        public Mat Transform
        {
            get => transform;
            set
            {
                transform = value;
                //shader.Uniforms["transform"].SetValue(transform.m);
            }
        }

        public PrimitiveBatchOLD()
        {
            currentCapacity = initialCapacity;
            vertices = new Vertex[currentCapacity];

            GL.GenVertexArrays(1, out vao);

            GL.GenBuffers(1, out vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * 6 * 4, vertices, BufferUsageHint.StreamDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            Projection = Mat.Identity;
            Transform = Mat.Identity;

            ResetAttributes();
        }

        public void ResetAttributes()
        {
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            /*GL.VertexAttribPointer(shader.positionAttribute, 2, VertexAttribPointerType.Float, false, 6 * 4, 0);
            GL.EnableVertexAttribArray(shader.positionAttribute);

            GL.VertexAttribPointer(shader.colorAttribute, 4, VertexAttribPointerType.Float, false, 6 * 4, 2 * 4);
            GL.EnableVertexAttribArray(shader.colorAttribute);
            */
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void Begin()
        {
            Flush();
            //GL.ShadeModel(ShadingModel.Flat);
        }

        public void End()
        {
            Flush();
            //GL.ShadeModel(ShadingModel.Smooth);
        }

        public void Flush()
        {
            if (pointer == 0)
                return;

            //GL.UseProgram(shader.Program);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BindVertexArray(vao);

            if (isDirty)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * 6 * 4, vertices, BufferUsageHint.StreamDraw);
                isDirty = false;
            }
            else
            {
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, pointer * 6 * 4, vertices);
            }

            //GL.DrawArrays(currentType, 0, pointer);

            GL.UseProgram(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            pointer = 0;
        }

        public void AddVertex(float x, float y, Color4 color)
        {
            if (pointer == vertices.Length)
                EnsureArraysCapacity();

            vertices[pointer++].Set(x, y, color);
        }

        private void EnsureArraysCapacity()
        {
            currentCapacity += initialCapacity;

            var newVertices = new Vertex[currentCapacity];
            vertices.CopyTo(newVertices, 0);
            vertices = newVertices;
            isDirty = true;
        }

        protected struct Vertex
        {
            float x, y, r, g, b, a;

            public void Set(float x, float y, Color4 c)
            {
                this.x = x;
                this.y = y;
                r = c.R;
                g = c.G;
                b = c.B;
                a = c.A;
            }

            public Vertex SetColor(Color4 c)
            {
                r = c.R;
                g = c.G;
                b = c.B;
                a = c.A;
                return this;
            }

            public Vertex SetPosition(float x, float y)
            {
                this.x = x;
                this.y = y;
                return this;
            }
        }
    }
}
