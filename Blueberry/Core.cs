using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry
{
    public partial class Core
    {
        public Graphics graphics;
        public Col BackgroundColor { get; set; } = Col.Black;
        public bool ForceSceneChange { get; set; } = false;

        public Scene Scene
        {
            get
            {
                return currentScene;
            }
            set
            {
                nextScene = value;
                if (ForceSceneChange)
                    ChangeScene();
            }
        }

        protected Scene currentScene, nextScene;

        protected void ChangeScene()
        {
            if (nextScene != null)
            {
                if (currentScene != null)
                    currentScene.End();
                currentScene = nextScene;
                nextScene = null;

                currentScene.graphics = graphics;
                currentScene.Initialize();
                currentScene.OnResize(Width, Height);
                currentScene.Begin();
            }
        }

        public virtual void Load()
        {

        }
    }
}
