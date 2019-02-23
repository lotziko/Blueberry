using System;
using System.Collections.Generic;
using System.Text;

namespace BlueberryTexturePackerCore
{
    public class Core : Blueberry.Core
    {
        public Core()
        {
            ForceSceneChange = true;
            BackgroundColor = new Blueberry.Col(42, 42, 45, 255);
            Scene = new MainScene();
        }
    }
}
