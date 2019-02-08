using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTKTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var gw = new MainCore())
            {
                gw.VSync = VSyncMode.Adaptive;
                /*gw.RenderFrame += (sender, e) =>
                {
                    //gw.SwapBuffers();
                    Thread.Sleep(16);
                };*/
                gw.Run(60);
            }
        }
    }
}
