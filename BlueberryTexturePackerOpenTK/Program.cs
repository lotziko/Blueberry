using BlueberryTexturePackerCore;
using System;

namespace BlueberryTexturePackerOpenTK
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var core = new Core())
            {
                core.Run(60);
            }
        }
    }
}
