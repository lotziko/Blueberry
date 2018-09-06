using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace BlueberryCore
{
    public static class Input
    {
        private static long currentEventTime;
        private static Point mousePos = Point.Zero, previousMousePos = Point.Zero;
        private static readonly List<MouseButtonState> mouseButtonStates = new List<MouseButtonState>(6)
        {
            new MouseButtonState(MouseButton.LEFT),
            new MouseButtonState(MouseButton.RIGHT),
            new MouseButtonState(MouseButton.MIDDLE),
            new MouseButtonState(MouseButton.XB1),
            new MouseButtonState(MouseButton.XB2),
            new MouseButtonState(MouseButton.ANY)
        };

        private static IInputProcessor _processor;

        public static void SetInputProcessor(IInputProcessor processor) => _processor = processor;

        public static void Update()
        {
            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();
            CurrentKeyboardState = Keyboard.GetState();
            
            mousePos = CurrentMouseState.Position;
            previousMousePos = PreviousMouseState.Position;

            UpdateMouseButtonsState();

            if (_processor != null)
            {
                var pressedKeys = CurrentKeyboardState.GetPressedKeys();
                var previousPressedKeys = PreviousKeyboardState.GetPressedKeys();

                for (int i = 0; i < pressedKeys.Length; i++)
                {
                    if (!previousPressedKeys.Contains(pressedKeys[i]))
                    {
                        _processor.KeyDown((int)pressedKeys[i]);
                        CheckEventTime();
                    }
                }

                for (int i = 0; i < previousPressedKeys.Length; i++)
                {
                    if (!pressedKeys.Contains(previousPressedKeys[i]))
                    {
                        _processor.KeyUp((int)previousPressedKeys[i]);
                        CheckEventTime();
                    }
                }

                if (MouseWheelDelta != 0)
                {
                    _processor.Scrolled(MouseWheelDelta);
                    CheckEventTime();
                }

                for (int i = 0; i < 5; i++)
                {
                    var btn = mouseButtonStates[i];
                    if (btn.pressed)
                    {
                        _processor.TouchDown(mousePos.X, mousePos.Y, 0, (int)btn.button);
                        CheckEventTime();
                    }
                    else if (btn.released)
                    {
                        _processor.TouchUp(mousePos.X, mousePos.Y, 0, (int)btn.button);
                        CheckEventTime();
                    }
                }

                if (mousePos != previousMousePos)
                {
                    _processor.MouseMoved(mousePos.X, mousePos.Y);
                    CheckEventTime();

                    if (IsMouseButtonDown(MouseButton.ANY))
                    {
                        _processor.TouchDragged(mousePos.X, mousePos.Y, 0);
                        CheckEventTime();
                    }
                }
            }
        }
        
        private static void CheckEventTime()
        {
            var nanos = TimeUtils.NanoTime();
            currentEventTime = nanos | nanos & 0xFFFFFFFFL;
        }

        public static void TextInput(object sender, TextInputEventArgs e)
        {
            if (_processor != null)
            {
                _processor.KeyTyped((int)e.Key, e.Character);
                CheckEventTime();
            }
        }

        public static KeyboardState CurrentKeyboardState { get; private set; }

        public static KeyboardState PreviousKeyboardState { get; private set; }

        public static MouseState CurrentMouseState { get; private set; }

        public static MouseState PreviousMouseState { get; private set; }

        public static bool IsKeyDown(Keys key)
        {
            return CurrentKeyboardState.IsKeyDown(key);
        }

        public static bool IsKeyReleased(Keys key)
        {
            return CurrentKeyboardState.IsKeyUp(key);
        }

        public static bool IsKeyPressed(Keys key)
        {
            return (CurrentKeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyUp(key));
        }

        /// <summary>
        /// Get touch state
        /// </summary>
        /// <param name="id">ID - 2, because 0 and 1 are reserved and not used</param>
        /// <returns></returns>
        public static bool IsTouched(int id)
        {
            var state = TouchPanel.GetState();
            if (!state.IsConnected)
                return false;
            foreach(TouchLocation tl in state)
            {
                if (tl.Id - 2 == id)
                    return true;
            }
            return false;
        }

        public static bool IsTouched()
        {
            var state = TouchPanel.GetState();
            return (state.IsConnected && state.Count > 0) || IsMouseButtonDown(MouseButton.ANY);
        }

        public static long GetCurrentEventTime()
        {
            return currentEventTime;
        }

        public static int MouseWheelDelta
        {
            get
            {
                return Math.Sign(PreviousMouseState.ScrollWheelValue - CurrentMouseState.ScrollWheelValue);
            }
        }

        public static bool IsMouseButtonDown(MouseButton button) => mouseButtonStates[(int)button].down;

        public static bool IsMouseButtonPressed(MouseButton button) => mouseButtonStates[(int)button].pressed;

        public static bool IsMouseButtonReleased(MouseButton button) => mouseButtonStates[(int)button].released;

        public static bool IsMouseWheelUp()
        {
            bool result = (CurrentMouseState.ScrollWheelValue > PreviousMouseState.ScrollWheelValue);
            //previousMouseWheelValue = CurrentMouseState.ScrollWheelValue;
            return result;
        }

        public static bool IsMouseWheelDown()
        {
            bool result = (CurrentMouseState.ScrollWheelValue < PreviousMouseState.ScrollWheelValue);
            //previousMouseWheelValue = CurrentMouseState.ScrollWheelValue;
            return result;
        }

        public static Point GetRawMousePos()
        {
            mousePos.X = CurrentMouseState.X;
            mousePos.Y = CurrentMouseState.Y;
            return mousePos;
        }

        private static void UpdateMouseButtonsState()
        {
            mouseButtonStates[5].Reset();
            for(int i = 0; i < 5; i++)
            {
                var btn = mouseButtonStates[i];
                btn.Reset();
                var state = GetState(CurrentMouseState, btn.button).Value;
                var oldState = GetState(PreviousMouseState, btn.button).Value;
                if (state == ButtonState.Pressed && oldState == ButtonState.Released)
                {
                    btn.pressed = true;
                    mouseButtonStates[5].pressed = true;
                }
                else if (state == ButtonState.Released && oldState == ButtonState.Pressed)
                {
                    btn.released = true;
                    mouseButtonStates[5].released = true;
                }
                else if (state == ButtonState.Pressed)
                {
                    btn.down = true;
                    mouseButtonStates[5].down = true;
                }
            }
        }

        private static ButtonState? GetState(MouseState mouseState, MouseButton button)
        {
            switch (button)
            {
                case MouseButton.LEFT:
                    return mouseState.LeftButton;
                case MouseButton.RIGHT:
                    return mouseState.RightButton;
                case MouseButton.MIDDLE:
                    return mouseState.MiddleButton;
                case MouseButton.XB1:
                    return mouseState.XButton1;
                case MouseButton.XB2:
                    return mouseState.XButton2;
            }
            return null;
        }
    }

    internal class MouseButtonState
    {
        public MouseButton button;
        public bool down, pressed, released;
        
        public void Reset()
        {
            down = false;
            pressed = false;
            released = false;
        }

        public MouseButtonState(MouseButton button)
        {
            this.button = button;
        }
    }

    public enum MouseButton
    {
        LEFT, RIGHT, MIDDLE, XB1, XB2, ANY
    }
}
