using OpenTK.Graphics.OpenGL4;
using System;

namespace Blueberry.OpenGL
{
    public enum VertexElementFormat
    {
        Single, Vector2, Vector3, Vector4, Color
    }

    public static class VertexElementFormatExt
    {
        public static VertexAttribPointerType GetVertexPointerType(this VertexElementFormat f)
        {
            switch (f)
            {
                case VertexElementFormat.Single:
                    return VertexAttribPointerType.Float;

                case VertexElementFormat.Vector2:
                    return VertexAttribPointerType.Float;

                case VertexElementFormat.Vector3:
                    return VertexAttribPointerType.Float;

                case VertexElementFormat.Vector4:
                    return VertexAttribPointerType.Float;

                case VertexElementFormat.Color:
                    return VertexAttribPointerType.Float;
            }

            throw new ArgumentException();
        }

        public static int GetNumberOfElements(this VertexElementFormat f)
        {
            switch (f)
            {
                case VertexElementFormat.Single:
                    return 1;

                case VertexElementFormat.Vector2:
                    return 2;

                case VertexElementFormat.Vector3:
                    return 3;

                case VertexElementFormat.Vector4:
                    return 4;

                case VertexElementFormat.Color:
                    return 4;
            }

            throw new ArgumentException();
        }

        public static int GetSize(this VertexElementFormat f)
        {
            switch (f)
            {
                case VertexElementFormat.Single:
                    return 4;

                case VertexElementFormat.Vector2:
                    return 8;

                case VertexElementFormat.Vector3:
                    return 12;

                case VertexElementFormat.Vector4:
                    return 16;

                case VertexElementFormat.Color:
                    return 16;
            }

            throw new ArgumentException();
        }
    }
}
