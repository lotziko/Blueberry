
namespace Blueberry
{
    public interface IInputProcessor
    {
        bool KeyDown(int keycode);

        bool KeyUp(int keycode);

        bool KeyTyped(int keycode, char character);

        bool TouchDown(int screenX, int screenY, int pointer, int button);

        bool TouchUp(int screenX, int screenY, int pointer, int button);

        bool TouchDragged(int screenX, int screenY, int pointer);

        bool MouseMoved(int screenX, int screenY);

        bool Scrolled(float amountX, float amountY);
    }
}
