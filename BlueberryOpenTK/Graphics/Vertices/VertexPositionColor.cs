using OpenTK;
using OpenTK.Graphics;

namespace Blueberry.OpenGL
{
    public struct VertexPositionColor : IVertexType
    {
        public Vector3 Position;
        public Color4 Color;

        public static readonly VertexDeclaration VertexDeclaration;

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }

        public VertexPositionColor(Vector3 position, Color4 color)
        {
            Position = position;
            Color = color;
        }

        public void Set(float x, float y, Color4 c)
        {
            Position.X = x;
            Position.Y = y;
            Color = c;
        }

        static VertexPositionColor()
        {
            VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0));
        }
    }
}
