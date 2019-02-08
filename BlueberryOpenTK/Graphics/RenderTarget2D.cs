using System;
using Blueberry;
using OpenTK.Graphics.OpenGL4;

namespace BlueberryOpenTK
{
    public class RenderTarget2D : Texture2D
    {
        internal int framebufferPointer;

        public Mat DefaultProjection { get; }

        public RenderTarget2D(int width, int height) : base(width, height)
        {
            GL.GenFramebuffers(1, out framebufferPointer);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferPointer);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texturePointer, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            DefaultProjection = new Mat(OpenTK.Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1));
        }

        internal static void Bind(RenderTarget2D target)
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, target == null ? 0 : target.framebufferPointer);
        }
    }
}
