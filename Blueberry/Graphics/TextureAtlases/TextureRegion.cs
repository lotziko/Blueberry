using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry
{
    public partial class TextureRegion
    {
        private Rect source;
        private Rect destination = new Rect();
        private Vec2 offset = Vec2.Zero;
        private static Vec2 position = new Vec2();
        private static Rect tmpSrc = new Rect();

        public Rect Source => source;

        public int Width => (int)source.Width;
        public int Height => (int)source.Height;
    }
}
