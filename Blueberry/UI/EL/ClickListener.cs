using System;

namespace Blueberry.UI
{
    public abstract class ClickListener : InputListener
    {
        public static float visualPressedDuration = 0.1f;

        private float tapSquareSize = 14, touchDownX = -1, touchDownY = -1;
        private int pressedPointer = -1;
        private int pressedButton = -1;
        private int button;
        private bool pressed, over, cancelled;
        private long visualPressedTime;
        private long tapCountInterval = (long)(0.4f * 1000);//millis//(long)(0.4f * 1000000000L);
        private int tapCount;
        private long lastTapTime;

        private static Vec2 tmpCoords;

        public ClickListener()
        {

        }

        public ClickListener(int button)
        {
            this.button = button;
        }

        public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
        {
            if (pressed) return false;
            if (pointer == 0 && this.button != -1 && button != this.button) return false;
            pressed = true;
            pressedPointer = pointer;
            pressedButton = button;
            touchDownX = x;
            touchDownY = y;
            visualPressedTime = TimeUtils.CurrentTimeMillis() + (long)(visualPressedDuration * 1000);
            return true;
        }

        public override void TouchDragged(InputEvent ev, float x, float y, int pointer)
        {
            if (pointer != pressedPointer || cancelled) return;
            pressed = IsOver(ev.GetListenerElement(), x, y);
            if (!pressed)
            {
                // Once outside the tap square, don't use the tap square anymore.
                InvalidateTapSquare();
            }
        }

        public override void TouchUp(InputEvent ev, float x, float y, int pointer, int button)
        {
            if (pointer == pressedPointer)
            {
                if (!cancelled)
                {
                    bool touchUpOver = IsOver(ev.GetListenerElement(), x, y);
                    // Ignore touch up if the wrong mouse button.
                    if (touchUpOver && pointer == 0 && this.button != -1 && button != this.button) touchUpOver = false;
                    if (touchUpOver)
                    {
                        long time = TimeUtils.CurrentTimeMillis();
                        if (time - lastTapTime > tapCountInterval) tapCount = 0;
                        tapCount++;
                        lastTapTime = time;
                        Clicked(ev, x, y);
                    }
                }
                pressed = false;
                pressedPointer = -1;
                pressedButton = -1;
                cancelled = false;
            }
        }
        
        public override void Enter(InputEvent ev, float x, float y, int pointer, Element fromElement)
        {
            if (pointer == -1 && !cancelled) over = true;
        }

        public override void Exit(InputEvent ev, float x, float y, int pointer, Element toElement)
        {
            if (pointer == -1 && !cancelled) over = false;
        }

        public void Cancel()
        {
            if (pressedPointer == -1) return;
            cancelled = true;
            pressed = false;
        }

        public virtual void Clicked(InputEvent ev, float x, float y)
        {

        }

        public bool IsOver(Element element, float x, float y)
        {
            tmpCoords.Set(x, y);
            var hit = element.Hit(tmpCoords.X, tmpCoords.Y, true);
            if (hit == null || !hit.IsDescendantOf(element)) return InTapSquare(x, y);
            return true;
        }

        public bool InTapSquare(float x, float y)
        {
            if (touchDownX == -1 && touchDownY == -1) return false;
            return Math.Abs(x - touchDownX) < tapSquareSize && Math.Abs(y - touchDownY) < tapSquareSize;
        }

        public bool InTapSquare()
        {
            return touchDownX != -1;
        }

        public void InvalidateTapSquare()
        {
            touchDownX = -1;
            touchDownY = -1;
        }

        public bool IsPressed()
        {
            return pressed;
        }

        public bool IsVisualPressed()
        {
            if (pressed) return true;
            if (visualPressedTime <= 0) return false;
            if (visualPressedTime > TimeUtils.CurrentTimeMillis()) return true;
            visualPressedTime = 0;
            return false;
        }

        public bool IsOver()
        {
            return over || pressed;
        }

        public void SetTapSquareSize(float halfTapSquareSize)
        {
            tapSquareSize = halfTapSquareSize;
        }

        public float GetTapSquareSize()
        {
            return tapSquareSize;
        }

        public void SetTapCountInterval(float tapCountInterval)
        {
            this.tapCountInterval = (long)(tapCountInterval * 1000000000L);
        }

        public int GetTapCount()
        {
            return tapCount;
        }

        public void SetTapCount(int tapCount)
        {
            this.tapCount = tapCount;
        }

        public float GetTouchDownX()
        {
            return touchDownX;
        }

        public float GetTouchDownY()
        {
            return touchDownY;
        }

        public int GetPressedButton()
        {
            return pressedButton;
        }

        public int GetPressedPointer()
        {
            return pressedPointer;
        }

        public int GetButton()
        {
            return button;
        }

        public void SetButton(int button)
        {
            this.button = button;
        }
    }

    public class ClickListener<T> : ClickListener
    {
        protected readonly T par;

        public ClickListener(T par, int button) : base(button)
        {
            this.par = par;
        }

        public ClickListener(T par) : base()
        {
            this.par = par;
        }
    }
}
