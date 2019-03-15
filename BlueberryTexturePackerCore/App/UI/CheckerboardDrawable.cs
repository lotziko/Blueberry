using Blueberry;
using Blueberry.UI;

namespace BlueberryTexturePackerCore
{
    public class CheckerboardDrawable : IDrawable
    {
        public float LeftWidth { get => 0; set => throw new System.NotImplementedException(); }
        public float RightWidth { get => 0; set => throw new System.NotImplementedException(); }
        public float TopHeight { get => 0; set => throw new System.NotImplementedException(); }
        public float BottomHeight { get => 0; set => throw new System.NotImplementedException(); }
        public float MinWidth { get => 0; set => throw new System.NotImplementedException(); }
        public float MinHeight { get => 0; set => throw new System.NotImplementedException(); }

        private readonly Texture tex;

        public CheckerboardDrawable(Col first, Col second, int size)
        {
            int doubleSize = size * 2;
            var data = new Col[doubleSize * doubleSize];
            for(int i = 0; i < doubleSize; i++)
            {
                for (int j = 0; j < doubleSize; j++)
                {
                    int m = (i / size + j / size) % 2;
                    data[i + j * doubleSize] = (m > 0 ? first : second);
                }
            }
            tex = new Texture(data, doubleSize, doubleSize, false, TextureWrap.Repeat);
        }

        public void Draw(Graphics graphics, float x, float y, float width, float height, Col color)
        {
            graphics.DrawTextureTiled(tex, x, y, width, height);
        }

        public void SetPadding(float top, float bottom, float left, float right)
        {
            throw new System.NotImplementedException();
        }
    }
}
