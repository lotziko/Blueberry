using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Reflection;

namespace BlueberryCore.InputModels
{
    public class StandardInputModel : IInputModel
    {
        private KeyboardState keyboardState;
        private MouseState mouseState;
        private int previousMouseWheelY;
        private int previousMouseWheelX;
        private List<(MouseButton button, PropertyInfo info)> mouseButtonProperties;

        public void Initialize()
        {
            mouseButtonProperties = new List<(MouseButton, PropertyInfo)>();
            var properties = typeof(MouseState).GetProperties();
            foreach (var info in properties)
            {
                if (info.PropertyType == typeof(ButtonState))
                {
                    mouseButtonProperties.Add((GetMouseButtonFromProperty(info.Name), info));
                }
            }
        }

        public void Update()
        {
            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();

            OnKeyDown();
            OnKeyUp();
            MouseMotion();
            OnMouseWheel();
            OnMouseButtonDownUp();
        }

        private void OnKeyDown()
        {
            var pressedKeys = keyboardState.GetPressedKeys();

            foreach (var key in pressedKeys)
            {
                Input.CheckEventTime();

                if (!Input.keyStates.ContainsKey(key))
                {
                    var state = Pool<InputState>.Obtain();
                    state.eventFrame = TimeUtils.CurrentFrame();
                    Input.keyStates.Add(key, state);
                }

                if (Input.Processor != null)
                {
                    Input.Processor.KeyDown((int)key);
                }
            }
        }

        private void OnKeyUp()
        {
            var pressedKeys = keyboardState.GetPressedKeys();

            lock(Input.keyStates.Keys)
            foreach (var key in Input.keyStates.Keys)
            {
                if (pressedKeys.Contains(key))
                {
                    Input.CheckEventTime();

                    Input.keyStates.TryGetValue(key, out InputState state);
                    if (state != null)
                    {
                        Input.keyStates.Remove(key);
                        Pool<InputState>.Free(state);
                    }

                    if (Input.Processor != null)
                    {
                        Input.Processor.KeyUp((int)key);
                    }
                }
            }
        }

        private void MouseMotion()
        {
            var position = mouseState.Position;

            if (position == Input.previousMousePos)
                return;

            Input.CheckEventTime();
            Input.previousMousePos = Input.mousePos;
            Input.mousePos.X = position.X;
            Input.mousePos.Y = position.Y;

            if (Input.Processor != null)
            {
                if (Input.mouseStates.Count > 0)
                    Input.Processor.TouchDragged(position.X, position.Y, 0);
                else
                    Input.Processor.MouseMoved(position.X, position.Y);

            }
        }

        private void OnMouseWheel()
        {
            var mouseWheelY = mouseState.ScrollWheelValue;
            var mouseWheelX = mouseState.HorizontalScrollWheelValue;

            if (mouseWheelX != previousMouseWheelX)
            {
                Input.CheckEventTime();
                Input.mouseWheelX = mouseWheelX - previousMouseWheelX;

                previousMouseWheelX = mouseWheelX;
            }

            if (mouseWheelY != previousMouseWheelY)
            {
                Input.CheckEventTime();
                Input.mouseWheelY = (mouseWheelY - previousMouseWheelY) / 120;

                if (Input.Processor != null && Input.mouseWheelY != 0)
                    Input.Processor.Scrolled(Input.mouseWheelY);

                previousMouseWheelY = mouseWheelY;
            }
        }

        private void OnMouseButtonDownUp()
        {
            foreach(var (button, info) in mouseButtonProperties)
            {
                var bstate = (ButtonState)info.GetValue(mouseState);
                if (bstate == ButtonState.Pressed)
                {
                    if (!Input.mouseStates.ContainsKey(button))
                    {
                        Input.CheckEventTime();

                        var state = Pool<InputState>.Obtain();
                        state.eventFrame = TimeUtils.CurrentFrame();
                        Input.mouseStates.Add(button, state);

                        if (Input.Processor != null)
                            Input.Processor.TouchDown(Input.mousePos.X, Input.mousePos.Y, 0, (int)button);
                    }
                }
                else
                {
                    if (Input.mouseStates.ContainsKey(button))
                    {
                        Input.CheckEventTime();

                        Input.mouseStates.TryGetValue(button, out InputState state);
                        if (state != null)
                        {
                            Input.mouseStates.Remove(button);
                            Pool<InputState>.Free(state);
                        }

                        if (Input.Processor != null)
                            Input.Processor.TouchUp(Input.mousePos.X, Input.mousePos.Y, 0, (int)button);
                    }
                }
            }
        }




        private static MouseButton GetMouseButtonFromProperty(string name)
        {
            switch (name)
            {
                case "LeftButton":
                    return MouseButton.LEFT;
                case "RightButton":
                    return MouseButton.RIGHT;
                case "MiddleButton":
                    return MouseButton.MIDDLE;
                case "XButton1":
                    return MouseButton.XB1;
                case "XButton2":
                    return MouseButton.XB2;
                default:
                    return default;
            }
        }
    }
}
