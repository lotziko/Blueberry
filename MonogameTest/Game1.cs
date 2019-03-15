using Blueberry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonogameTest
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Core
    {
        public override void Initialize()
        {
            base.Initialize();
            Scene = new Sc();
        }

        private class Sc : Scene
        {
            private Texture2D triangle;

            public override void Initialize()
            {
                base.Initialize();
                triangle = Content.Load<Texture2D>("triangle");
            }

            public override void Render()
            {
                base.Render();
                graphics.Clear(Col.Black);
                graphics.DrawTexture(triangle, 0, 0);
                graphics.Flush();
            }
        }
    }
}
