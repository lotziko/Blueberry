using System;
using Blueberry;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OpenTKTest
{
    public class Core : GameWindow
    {
        Graphics graphics;

        public Core()
        {
            //Screen.UpdateSize(Width, Height);
            graphics = new Graphics();
            
            //target = new RenderTarget2D(100, 100);
            //graphics.SetRenderTarget(target);
            //graphics.Clear(System.Drawing.Color.Red);
            //graphics.SetRenderTarget(null);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // Screen.UpdateSize(Width, Height);
            GL.Viewport(0, 0, Width, Height);

            //GL.MatrixMode(MatrixMode.Projection);
            //GL.LoadIdentity();
            //var matrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45), Width / Height, 1.0f, 100.0f);
            //GL.LoadMatrix(ref matrix);
            //GL.Ortho(0, Width, Height, 0, -1, 1);
            //GL.MatrixMode(MatrixMode.Modelview);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            //GL.Flush();
            GL.Clear(ClearBufferMask.ColorBufferBit);

            //graphics.Begin();
            for(int i = 0; i < 1000; i++)
                graphics.DrawRectangle(0, 0, 0.5f, 0.5f);
            graphics.End();

            //for(int i = 0; i < 50; i++)
            //    graphics.DrawRectangle(0, 0, 100, 100, false, Col.Red);
            //graphics.End();
            //graphics.Draw(target, 0, 0, new System.Drawing.Rectangle(0, 0, 50, 30));

            Title = (GC.GetTotalMemory(false) / 1048576f).ToString("F") + " MB";
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Console.WriteLine(e.Button);
        }
    }
}
