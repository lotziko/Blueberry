
using System;

namespace BlueberryCore
{
    public class InputAdapter : IInputProcessor
    {
        public virtual bool KeyDown(int keycode)
        {
            return false;
        }

        public virtual bool KeyTyped(int keycode, char character)
        {
            return false;
        }

        public virtual bool KeyUp(int keycode)
        {
            return false;
        }

        public virtual bool MouseMoved(int screenX, int screenY)
        {
            return false;
        }

        public virtual bool Scrolled(int amountX, int amountY)
        {
            return false;
        }

        public virtual bool TouchDown(int screenX, int screenY, int pointer, int button)
        {
            return false;
        }

        public virtual bool TouchDragged(int screenX, int screenY, int pointer)
        {
            return false;
        }

        public virtual bool TouchUp(int screenX, int screenY, int pointer, int button)
        {
            return false;
        }
    }
}
