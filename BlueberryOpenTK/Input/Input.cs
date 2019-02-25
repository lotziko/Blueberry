
namespace Blueberry
{
    public static partial class Input
    {
        private static float previousWheelX, previousWheelY;
        private static bool shift, alt, ctrl;

        internal static void Initialize(OpenTK.GameWindow core)
        {
            core.KeyDown += (sender, e) =>
            {
                if (e.Shift || e.Key.IsShift())
                    shift = true;
                if (e.Control || e.Key.IsCtrl())
                    ctrl = true;
                if (e.Alt || e.Key.IsAlt())
                    alt = true;

                switch (e.Key)
                {
                    case OpenTK.Input.Key.Back:
                        InputProcessor?.KeyTyped(0, '\b');
                        break;
                    default:
                        InputProcessor?.KeyDown((int)e.Key);
                        break;
                }
                KeyDown?.Invoke(core, new KeyEventArgs((Key)e.Key, e.Shift, e.Control, e.Alt));
            };
            core.KeyUp += (sender, e) =>
            {
                if (e.Shift || e.Key.IsShift())
                    shift = false;
                if (e.Control || e.Key.IsCtrl())
                    ctrl = false;
                if (e.Alt || e.Key.IsAlt())
                    alt = false;

                InputProcessor?.KeyUp((int)e.Key);
                KeyUp?.Invoke(core, new KeyEventArgs((Key)e.Key, e.Shift, e.Control, e.Alt));
            };
            core.KeyPress += (sender, e) =>
            {
                InputProcessor?.KeyTyped(0, e.KeyChar);
                KeyPress?.Invoke(core, new KeyPressEventArgs(e.KeyChar));
            };
            core.MouseDown += (sender, e) =>
            {
                InputProcessor?.TouchDown(e.X, e.Y, 0, (int)e.Button);
                MouseDown?.Invoke(core, new MouseButtonEventArgs((MouseButton)e.Button, e.X, e.Y));
            };
            core.MouseUp += (sender, e) =>
            {
                InputProcessor?.TouchUp(e.X, e.Y, 0, (int)e.Button);
                MouseUp?.Invoke(core, new MouseButtonEventArgs((MouseButton)e.Button, e.X, e.Y));
            };
            core.MouseMove += (sender, e) =>
            {
                if (OpenTK.Input.Mouse.GetState().LeftButton == OpenTK.Input.ButtonState.Pressed)
                {
                    InputProcessor?.TouchDragged(e.X, e.Y, 0);
                }
                else
                    InputProcessor?.MouseMoved(e.X, e.Y);
                MouseMove?.Invoke(core, new MouseMoveEventArgs(e.X, e.Y));
            };
            core.MouseWheel += (sender, e) =>
            {
                var deltaX = e.Mouse.Scroll.X - previousWheelX;
                var deltaY = e.Mouse.Scroll.Y - previousWheelY;
                InputProcessor?.Scrolled(deltaX, deltaY);
                previousWheelX = e.Mouse.Scroll.X;
                previousWheelY = e.Mouse.Scroll.Y;
                MouseWheel?.Invoke(core, new ScrollEventArgs(deltaX, deltaY));
            };
        }

        internal static long GetCurrentEventTime()
        {
            return 0;
        }

        public static bool IsTouched(int pointer)
        {
            if (pointer == 0)
                return OpenTK.Input.Mouse.GetState().IsAnyButtonDown;
            return false;
        }

        public static bool IsShiftDown()
        {
            return shift;
        }

        public static bool IsCtrlDown()
        {
            return ctrl;
        }

        public static bool IsAltDown()
        {
            return alt;
        }

        public static bool IsKeyDown(Key key)
        {
            return OpenTK.Input.Keyboard.GetState().IsKeyDown((OpenTK.Input.Key)key);
        }

        private static bool IsShift(this Key key)
        {
            return (key == Key.ShiftLeft || key == Key.ShiftRight);
        }

        private static bool IsCtrl(this Key key)
        {
            return (key == Key.ControlLeft || key == Key.ControlRight);
        }

        private static bool IsAlt(this Key key)
        {
            return (key == Key.AltLeft || key == Key.AltRight);
        }

        private static bool IsShift(this OpenTK.Input.Key key)
        {
            return (key == OpenTK.Input.Key.ShiftLeft || key == OpenTK.Input.Key.ShiftRight);
        }

        private static bool IsCtrl(this OpenTK.Input.Key key)
        {
            return (key == OpenTK.Input.Key.ControlLeft || key == OpenTK.Input.Key.ControlRight);
        }

        private static bool IsAlt(this OpenTK.Input.Key key)
        {
            return (key == OpenTK.Input.Key.AltLeft || key == OpenTK.Input.Key.AltRight);
        }
    }
}
