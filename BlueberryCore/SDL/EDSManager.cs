using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.InteropServices;

namespace BlueberryCore.SDL
{
    public static class EDSManager
    {
        public static EventHandler<KeyDownEventArgs> KeyDown;
        public static EventHandler<KeyUpEventArgs> KeyUp;
        public static EventHandler<MouseMotionEventArgs> MouseMotion;
        public static EventHandler<MouseWheelEventArgs> MouseWheel;
        public static EventHandler<MouseButtonDownEventArgs> MouseButtonDown;
        public static EventHandler<MouseButtonUpEventArgs> MouseButtonUp;
        public static EventHandler<Microsoft.Xna.Framework.TextInputEventArgs> TextInput;
        private static readonly SDL.SDL_EventFilter callback;

        static EDSManager()
        {
            callback = (IntPtr data, SDL.SDL_Event ev) =>
            {
                switch(ev.type)
                {
                    case SDL.SDL_EventType.SDL_TEXTINPUT:

                        int len = 0;
                        string text = string.Empty;
                        unsafe
                        {
                            while (Marshal.ReadByte((IntPtr)ev.text.text, len) != 0)
                            {
                                len++;
                            }
                            var buffer = new byte[len];
                            Marshal.Copy((IntPtr)ev.text.text, buffer, 0, len);
                            text = System.Text.Encoding.UTF8.GetString(buffer);
                        }
                        if (text.Length == 0)
                            break;
                        foreach (var c in text)
                        {
                            var key = KeyboardUtils.ToXna((int)c);
                            TextInput?.Invoke(null, new Microsoft.Xna.Framework.TextInputEventArgs(c, key));
                        }
                        
                        break;
                    case SDL.SDL_EventType.SDL_KEYDOWN:
                        KeyDown?.Invoke(null, new KeyDownEventArgs(KeyboardUtils.ToXna((int)ev.key.keysym.sym), (char)ev.key.keysym.sym));
                        break;
                    case SDL.SDL_EventType.SDL_KEYUP:
                        KeyUp?.Invoke(null, new KeyUpEventArgs(KeyboardUtils.ToXna((int)ev.key.keysym.sym), (char)ev.key.keysym.sym));
                        break;
                    case SDL.SDL_EventType.SDL_MOUSEMOTION:
                        MouseMotion?.Invoke(null, new MouseMotionEventArgs(ev.motion.x, ev.motion.y));
                        break;
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                        MouseButtonUp?.Invoke(null, new MouseButtonUpEventArgs(ev.button.x, ev.button.y, GetMouseButtonFromSDLButton(ev.button.button)));
                        break;
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                        MouseButtonDown?.Invoke(null, new MouseButtonDownEventArgs(ev.button.x, ev.button.y, GetMouseButtonFromSDLButton(ev.button.button)));
                        break;
                    case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                        MouseWheel?.Invoke(null, new MouseWheelEventArgs(ev.wheel.x, ev.wheel.y));
                        break;
                }
                return 0;
            };
            SDL.SDL_AddEventWatch(callback, IntPtr.Zero);
        }

        private static MouseButton GetMouseButtonFromSDLButton(byte button)
        {
            switch ((uint)button)
            {
                case SDL.SDL_BUTTON_LEFT:
                    return MouseButton.LEFT;
                case SDL.SDL_BUTTON_RIGHT:
                    return MouseButton.RIGHT;
                case SDL.SDL_BUTTON_MIDDLE:
                    return MouseButton.MIDDLE;
                case SDL.SDL_BUTTON_X1:
                    return MouseButton.XB1;
                case SDL.SDL_BUTTON_X2:
                    return MouseButton.XB2;
                default:
                    return MouseButton.ANY;
            }
        }
    }

    public class MouseButtonDownEventArgs : EventArgs
    {
        public int x, y;
        public MouseButton button;

        public MouseButtonDownEventArgs(int x, int y, MouseButton button)
        {
            this.x = x;
            this.y = y;
            this.button = button;
        }
    }

    public class MouseButtonUpEventArgs : EventArgs
    {
        public int x, y;
        public MouseButton button;

        public MouseButtonUpEventArgs(int x, int y, MouseButton button)
        {
            this.x = x;
            this.y = y;
            this.button = button;
        }
    }

    public class MouseMotionEventArgs : EventArgs
    {
        public int x, y;

        public MouseMotionEventArgs(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class MouseWheelEventArgs : EventArgs
    {
        public int x, y;

        public MouseWheelEventArgs(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class KeyDownEventArgs : EventArgs
    {
        public Keys key;
        public char character;

        public KeyDownEventArgs(Keys key, char character)
        {
            this.key = key;
            this.character = character;
        }
    }

    public class KeyUpEventArgs : EventArgs
    {
        public Keys key;
        public char character;

        public KeyUpEventArgs(Keys key, char character)
        {
            this.key = key;
            this.character = character;
        }
    }
}
