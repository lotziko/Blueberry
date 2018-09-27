using BlueberryCore.SDL;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;

namespace BlueberryCore
{
    public static class Input
    {
        private static long currentEventTime;
        private static InputModels.IInputModel model;
        internal static Point mousePos = Point.Zero, previousMousePos = Point.Zero;
        internal static int mouseWheelX, mouseWheelY;
        internal static readonly Dictionary<MouseButton, InputState> mouseStates = new Dictionary<MouseButton, InputState>();
        internal static readonly Dictionary<Keys, InputState> keyStates = new Dictionary<Keys, InputState>(); 
        
        internal static IInputProcessor Processor;

        public static void SetInputProcessor(IInputProcessor processor) => Processor = processor;
        
        public static void Initialize(InputModels.IInputModel iModel)
        {
            model = iModel;
            model.Initialize();
        }

        public static void Update()
        {
            model.Update();
        }
        
        internal static void CheckEventTime()
        {
            var nanos = TimeUtils.NanoTime();
            currentEventTime = nanos | nanos & 0xFFFFFFFFL;
        }
        
        public static bool IsKeyDown(Keys key)
        {
            return keyStates.ContainsKey(key);
        }

        public static bool IsKeyReleased(Keys key)
        {
            return !keyStates.ContainsKey(key);
        }

        public static bool IsKeyPressed(Keys key)
        {
            return keyStates.ContainsKey(key) && !keyStates[key].outdated;
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
            return (state.IsConnected && state.Count > 0) || mouseStates.Count > 0;
        }

        public static long GetCurrentEventTime()
        {
            return currentEventTime;
        }
        
        public static bool IsMouseButtonDown(MouseButton button)
        {
            return mouseStates.ContainsKey(button);
        }

        public static bool IsMouseButtonReleased(MouseButton button)
        {
            return !mouseStates.ContainsKey(button);
        }

        public static bool IsMouseButtonPressed(MouseButton button)
        {
            return mouseStates.ContainsKey(button) && !mouseStates[button].outdated;
        }

        public static bool IsMouseWheelUp()
        {
            return mouseWheelY < 0;
        }

        public static bool IsMouseWheelDown()
        {
            return mouseWheelY > 0;
        }

        public static bool IsMouseWheelLeft()
        {
            return mouseWheelX < 0;
        }

        public static bool IsMouseWheelRight()
        {
            return mouseWheelX > 0;
        }

        public static Point GetRawMousePos()
        {
            return mousePos;
        }
        
        /*public static ButtonState? GetState(MouseState mouseState, MouseButton button)
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
        }*/
    }
    
    internal class InputState : IPoolable
    {
        public bool outdated;
        public ulong eventFrame;

        public void Reset()
        {
            outdated = false;
            eventFrame = 0;
        }
    }

    public enum MouseButton
    {
        LEFT, RIGHT, MIDDLE, XB1, XB2, ANY
    }
}
