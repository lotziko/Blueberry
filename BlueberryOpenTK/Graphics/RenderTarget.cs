using System;
using Blueberry;
using OpenTK.Graphics.OpenGL4;

namespace Blueberry
{
    public partial class RenderTarget : Texture
    {
        internal int framebufferPointer;

        public Mat DefaultProjection { get; }

        public RenderTarget(int width, int height) : base(width, height)
        {
            GL.GenFramebuffers(1, out framebufferPointer);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferPointer);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texturePointer, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            DefaultProjection = new Mat(OpenTK.Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1));
        }
    }
}
