using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blueberry.OpenGL
{
    public class VertexElement
    {
        public int Offset { get; set; }
        public VertexElementFormat VertexElementFormat { get; set; }
        public VertexElementUsage VertexElementUsage { get; set; }
        public int UsageIndex { get; set; }

        public VertexElement(int offset, VertexElementFormat elementFormat, VertexElementUsage elementUsage, int usageIndex)
        {
            Offset = offset;
            VertexElementFormat = elementFormat;
            VertexElementUsage = elementUsage;
            UsageIndex = usageIndex;
        }
    }
}
