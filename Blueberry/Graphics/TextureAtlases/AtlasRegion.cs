using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry
{
    public partial class AtlasRegion : TextureRegion
    {
        internal string name;
        internal int index = -1, originalWidth, originalHeight, packedWidth, packedHeight, offsetX, offsetY;
        internal int[] splits, pads;
        internal bool rotate;

        /// <summary>
        /// Get name
        /// </summary>
        /// <returns></returns>

        public string Name => name;

        public override int Width => originalWidth;
        public override int Height => originalHeight;

        public AtlasRegion() : base()
        {

        }

        public override string ToString()
        {
            return name;
        }
    }
}
