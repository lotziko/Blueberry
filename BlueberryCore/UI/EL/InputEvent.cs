using Microsoft.Xna.Framework;

namespace BlueberryCore.UI
{
    public class InputEvent : Event
    {
        private InputType type;
        private float stageX, stageY;
        private int keyCode, scrollAmount;
        private int button, pointer;
        private char character;
        private Element relatedElement;

        public override void Reset()
        {
            base.Reset();
            relatedElement = null;
            button = -1;
        }

        /** The stage x coordinate where the event occurred. Valid for: touchDown, touchDragged, touchUp, mouseMoved, enter, and exit. */
        public float GetStageX()
        {
            return stageX;
        }

        public void SetStageX(float stageX)
        {
            this.stageX = stageX;
        }

        /** The stage x coordinate where the event occurred. Valid for: touchDown, touchDragged, touchUp, mouseMoved, enter, and exit. */
        public float GetStageY()
        {
            return stageY;
        }

        public void SetStageY(float stageY)
        {
            this.stageY = stageY;
        }

        public bool IsTouchFocusCancel()
        {
            return stageX == int.MinValue || stageY == int.MinValue;
        }

        /** The type of input event. */
        public InputType GetInputType()
        {
            return type;
        }

        public void SetInputType(InputType type)
        {
            this.type = type;
        }
        
        public int GetButton()
        {
            return button;
        }

        public void SetButton(int button)
        {
            this.button = button;
        }

        /** The pointer index for the event. The first touch is index 0, second touch is index 1, etc. Always -1 on desktop. Valid for: touchDown, touchDragged, touchUp, enter, and exit. */
        public int GetPointer()
        {
            return pointer;
        }

        public void SetPointer(int pointer)
        {
            this.pointer = pointer;
        }

        /** The key code of the key that was pressed. Valid for: keyDown and keyUp. */
        public int GetKeyCode()
        {
            return keyCode;
        }

        public void SetKeyCode(int keyCode)
        {
            this.keyCode = keyCode;
        }

        /** The character for the key that was type. Valid for: keyTyped. */
        public char GetCharacter()
        {
            return character;
        }

        public void SetCharacter(char character)
        {
            this.character = character;
        }

        /** The amount the mouse was scrolled. Valid for: scrolled. */
        public int GetScrollAmount()
        {
            return scrollAmount;
        }

        public void SetScrollAmount(int scrollAmount)
        {
            this.scrollAmount = scrollAmount;
        }

        /** The actor related to the event. Valid for: enter and exit. For enter, this is the actor being exited, or null. For exit,
         * this is the actor being entered, or null. */
        public Element GetRelatedElement()
        {
            return relatedElement;
        }

        /** @param relatedActor May be null. */
        public void SetRelatedElement(Element relatedElement)
        {
            this.relatedElement = relatedElement;
        }

        /** Sets actorCoords to this event's coordinates relative to the specified actor.
         * @param actorCoords Output for resulting coordinates. */
        public Vector2 ToCoordinates(Element element, Vector2 actorCoords)
        {
            actorCoords.X = stageX;
            actorCoords.Y = stageY;
            return element.StageToLocalCoordinates(actorCoords);
        }
    }

    public enum InputType
    {
        touchDown, mouseMoved, touchDragged, touchUp, scrolled, keyDown, keyTyped, keyUp, enter, exit
    }
}
