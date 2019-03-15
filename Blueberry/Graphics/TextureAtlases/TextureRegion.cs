using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry
{
    public partial class TextureRegion
    {
        protected float u, v;
        protected float u2, v2;
        protected int x, y;
        protected int regionWidth, regionHeight;

        public virtual float U => u;
        public virtual float V => v;
        public virtual int X => x;
        public virtual int Y => y;
        public virtual int Width => regionWidth;
        public virtual int Height => regionHeight;

        public TextureRegion()
        {

        }
    }
}
