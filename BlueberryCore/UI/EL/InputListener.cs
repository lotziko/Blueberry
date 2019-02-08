
using Microsoft.Xna.Framework;

namespace BlueberryCore.UI
{
    public class InputListener : IEventListener
    {
        private static Vector2 tmpCoords = Vector2.Zero;

        public bool Handle(Event e)
        {
            if (!(e is InputEvent)) return false;
            var ev = (InputEvent) e;

            switch(ev.GetInputType())
            {
                case InputType.keyDown:
                    KeyDown(ev, ev.GetKeyCode());
                    break;
                case InputType.keyTyped:
                    KeyTyped(ev, ev.GetKeyCode(), ev.GetCharacter());
                    break;
                case InputType.keyUp:
                    KeyUp(ev, ev.GetKeyCode());
                    break;
            }
            
            tmpCoords = ev.ToCoordinates(ev.GetListenerElement(), tmpCoords);

            switch(ev.GetInputType())
            {
                case InputType.touchDown:
                    TouchDown(ev, tmpCoords.X, tmpCoords.Y, ev.GetPointer(), ev.GetButton());
                    break;
                case InputType.touchUp:
                    TouchUp(ev, tmpCoords.X, tmpCoords.Y, ev.GetPointer(), ev.GetButton());
                    break;
                case InputType.mouseMoved:
                    MouseMoved(ev, tmpCoords.X, tmpCoords.Y);
                    break;
                case InputType.touchDragged:
                    TouchDragged(ev, tmpCoords.X, tmpCoords.Y, ev.GetPointer());
                    break;
                case InputType.scrolled:
                    Scrolled(ev, tmpCoords.X, tmpCoords.Y, ev.GetScrollAmountX(), ev.GetScrollAmountY());
                    break;
                case InputType.enter:
                    Enter(ev, tmpCoords.X, tmpCoords.Y, ev.GetPointer(), ev.GetRelatedElement());
                    break;
                case InputType.exit:
                    Exit(ev, tmpCoords.X, tmpCoords.Y, ev.GetPointer(), ev.GetRelatedElement());
                    break;
            }

            return true;
        }

        public virtual bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
        {
		    return false;
        }

        public virtual void TouchUp(InputEvent ev, float x, float y, int pointer, int button)
        {

        }

        public virtual bool MouseMoved(InputEvent ev, float x, float y)
        {
		    return false;
        }

        public virtual void TouchDragged(InputEvent ev, float x, float y, int pointer)
        {

        }

        public virtual bool Scrolled(InputEvent ev, float x, float y, int amountX, int amountY)
        {
            return false;
        }

        public virtual void Enter(InputEvent ev, float x, float y, int pointer, Element fromElement)
        {

        }

        public virtual void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
        {

	    }

        public virtual bool KeyDown(InputEvent ev, int keycode)
        {
            return false;
        }

        public virtual bool KeyTyped(InputEvent ev, int keycode, char character)
        {
            return false;
        }

        public virtual bool KeyUp(InputEvent ev, int keycode)
        {
            return false;
        }
    }
}
