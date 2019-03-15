using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Blueberry.OpenGL
{
    public class GraphicsDevice
    {
        internal int MaxVertexAttributes, MaxTextureSlots = 16;
        internal static readonly List<int> enabledAttributes = new List<int>();

        internal Effect Shader
        {
            get => shader;
            set
            {
                if (shader != value)
                {
                    shader = value;
                    if (shader == null)
                        GL.UseProgram(0);
                    else
                        GL.UseProgram(shader.Program);
                } 
            }
        }

        private Rect viewport;
        private Rect? scissorRectangle;
        private int primitiveRestartIndex;
        private Effect shader;
        private bool hasRenderTarget, scissorRectangleDirty;

        public TextureCollection Textures { get; private set; }
        public Rect Viewport
        {
            get => viewport;
            set
            {
                viewport = value;

                if (hasRenderTarget)
                    GL.Viewport((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);
                else
                    GL.Viewport((int)value.X, (int)(Screen.Height - (value.Y + value.Height)), (int)value.Width, (int)value.Height);
            }
        }

        public Rect? ScissorRectangle
        {
            get => scissorRectangle;
            set
            {
                if (scissorRectangle == value)
                    return;

                scissorRectangle = value;
                scissorRectangleDirty = true;
            }
        }

        public int PrimitiveRestartIndex
        {
            get => primitiveRestartIndex;
            set
            {
                primitiveRestartIndex = value;
                GL.Enable(EnableCap.PrimitiveRestart);
                GL.PrimitiveRestartIndex(value);
            }
        }

        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, ushort[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            ApplyState(true);

            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);
            
            var vertexAddr = (IntPtr)(vbHandle.AddrOfPinnedObject().ToInt64() + vertexDeclaration.VertexStride * vertexOffset);

            vertexDeclaration.GraphicsDevice = this;
            vertexDeclaration.Apply(Shader, vertexAddr);

            GL.DrawElements(primitiveType, GetElementCountArray(primitiveType, primitiveCount), DrawElementsType.UnsignedShort, (IntPtr)(ibHandle.AddrOfPinnedObject().ToInt64() + (indexOffset * sizeof(short))));
            
            ibHandle.Free();
            vbHandle.Free();
        }

        private void ApplyState(bool applyShaders)
        {
            if (scissorRectangleDirty)
            {
                if (scissorRectangle.HasValue)
                {
                    var rect = scissorRectangle.Value;

                    if (!hasRenderTarget)
                        rect.Y = Screen.Height - (rect.Y + rect.Height);

                    GL.Enable(EnableCap.ScissorTest);
                    GL.Scissor((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
                }
                else
                {
                    GL.Scissor(0, 0, Screen.Width, Screen.Height);
                    GL.Disable(EnableCap.ScissorTest);
                }

                scissorRectangleDirty = false;
            }

            if (!applyShaders)
                return;

            Textures.UpdateTextures();
        }

        public void SetRenderTarget(RenderTarget target)
        {
            int targetWidth, targetHeight;

            if (target == null)
            {
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
                targetWidth = Screen.Width;
                targetHeight = Screen.Height;
                hasRenderTarget = false;
            }
            else
            {
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, target.framebufferPointer);
                targetWidth = target.Width;
                targetHeight = target.Height;
                hasRenderTarget = true;
            }

            Viewport = new Rect(0, 0, targetWidth, targetHeight);
        }

        public void Clear(Color4 color)
        {
            ApplyState(false);
            GL.ClearColor(color);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        internal void SetVertexAttributeArray(bool[] attrs)
        {
            for (var i = 0; i < attrs.Length; i++)
            {
                if (attrs[i] && !enabledAttributes.Contains(i))
                {
                    enabledAttributes.Add(i);
                    GL.EnableVertexAttribArray(i);
                }
                else if (!attrs[i] && enabledAttributes.Contains(i))
                {
                    enabledAttributes.Remove(i);
                    GL.DisableVertexAttribArray(i);
                }
            }
        }

        private static int GetElementCountArray(PrimitiveType primitiveType, int primitiveCount)
        {
            switch (primitiveType)
            {
                case PrimitiveType.Lines:
                    return primitiveCount * 2;
                case PrimitiveType.LineStrip:
                    return primitiveCount + 1;
                case PrimitiveType.Triangles:
                    return primitiveCount * 3;
                case PrimitiveType.TriangleStrip:
                    return primitiveCount + 2;
            }

            throw new NotSupportedException();
        }

        public GraphicsDevice()
        {
            GL.GetInteger(GetPName.MaxVertexAttribs, out MaxVertexAttributes);
            GL.GetInteger(GetPName.MaxTextureImageUnits, out MaxTextureSlots);

            Textures = new TextureCollection(this, MaxTextureSlots);
        }
    }
}
