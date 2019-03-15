using System;

namespace Blueberry.OpenGL
{
    public enum VertexElementUsage
    {
        Position, Color, TextureCoordinate
    }

    public static class VertexElementUsageExt
    {
        public static string SemanticName(this VertexElementUsage v)
        {
            switch(v)
            {
                case VertexElementUsage.Position:
                    return "POSITION";
                case VertexElementUsage.Color:
                    return "COLOR";
                case VertexElementUsage.TextureCoordinate:
                    return "TEXCOORD";
            }

            throw new ArgumentException();
        }
    }
}
