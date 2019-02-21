using OpenTK.Graphics.OpenGL4;

namespace Blueberry
{
    internal static class TextureWrapExt
    {
        public static TextureWrapMode ToOpenTK(this TextureWrap value)
        {
            switch(value)
            {
                case TextureWrap.ClampToBorder:
                    return TextureWrapMode.ClampToBorder;
                case TextureWrap.ClampToEdge:
                    return TextureWrapMode.ClampToEdge;
                case TextureWrap.Repeat:
                    return TextureWrapMode.Repeat;
                default:
                    return TextureWrapMode.ClampToBorder;
            }
        }
    }
}
