using System;

namespace Blueberry
{
    public static partial class Input
    {
        public static IInputProcessor InputProcessor { get; set; }
    }

    public class KeyEventArgs : EventArgs
    {
        public readonly Key Key;
        public readonly bool Shift, Control, Alt;

        public KeyEventArgs(Key key, bool shift, bool control, bool alt)
        {
            Key = key;
            Shift = shift;
            Control = control;
            Alt = alt;
        }
    }

    public class ScrollEventArgs : EventArgs
    {
        public readonly float X, Y;

        public ScrollEventArgs(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public class KeyPressEventArgs : EventArgs
    {
        public readonly char Char;

        public KeyPressEventArgs(char character)
        {
            Char = character;
        }
    }

    public class MouseMoveEventArgs : EventArgs
    {
        public readonly int X, Y;

        public MouseMoveEventArgs(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class MouseButtonEventArgs : MouseMoveEventArgs
    {
        public readonly MouseButton Button;

        public MouseButtonEventArgs(MouseButton button, int x, int y) : base(x, y)
        {
            Button = button;
        }
    }
}
