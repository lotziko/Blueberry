
namespace Blueberry
{
    public interface IFont
    {
        float Ascent { get; }
        float Descent { get; }
        float LineHeight { get; }
        float SpaceWidth { get; }

        void Draw(Graphics graphics, string text, float x, float y, Col? color = null);

        void Draw(Graphics graphics, string text, float x, float y, int begin, int end, Col? color = null);

        void Draw(Graphics graphics, string text, float x, float y, int width, Col? color = null);

        string WrapText(string text, float maxLineWidth);

        string TruncateText(string text, string ellipsis, float maxLineWidth);

        int ComputeWidth(string text);

        int ComputeWidth(string text, int begin, int end);

        bool HasCharacter(char character);

        float GetCharacterAdvance(char character);

        Vec2 MeasureString(string text);
    }
}
