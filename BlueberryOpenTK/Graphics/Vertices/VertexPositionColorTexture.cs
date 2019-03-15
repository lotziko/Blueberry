using OpenTK;
using OpenTK.Graphics;

namespace Blueberry.OpenGL
{
    public struct VertexPositionColorTexture : IVertexType
    {
        public Vector3 Position;
        public Color4 Color;
        public Vector2 Texcoord;
        public static readonly VertexDeclaration VertexDeclaration;

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }

        public VertexPositionColorTexture(Vector3 position, Color4 color, Vector2 texcoord)
        {
            Position = position;
            Color = color;
            Texcoord = texcoord;
        }

        static VertexPositionColorTexture()
        {
            VertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
                );
        }

        public void Set(float x, float y, float u, float v, Color4 c)
        {
            Position.X = x;
            Position.Y = y;
            Texcoord.X = u;
            Texcoord.Y = v;
            Color = c;
        }
    }
}
