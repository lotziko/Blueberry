using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry
{
    public partial class TextureRegion
    {
        protected float u, v;
        protected float u2, v2;
        protected int regionWidth, regionHeight;

        public virtual int Width => regionWidth;
        public virtual int Height => regionHeight;

        public TextureRegion()
        {

        }
    }
}
