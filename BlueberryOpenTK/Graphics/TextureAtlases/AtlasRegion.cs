using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blueberry
{
    public partial class AtlasRegion : TextureRegion
    {
        public AtlasRegion(Texture texture, int x, int y, int width, int height) : base(texture, x, y, width, height)
        {
            originalWidth = width;
            originalHeight = height;
            packedWidth = width;
            packedHeight = height;
        }
    }
}
