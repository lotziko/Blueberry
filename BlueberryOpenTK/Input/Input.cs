
namespace Blueberry
{
    public static partial class Input
    {
        private static float previousWheelX, previousWheelY;
        private static bool shift, alt, ctrl;

        public static void Initialize(Core core)
        {
            core.KeyDown += Core_KeyDown;
            core.KeyUp += Core_KeyUp;
            core.KeyPress += Core_KeyPress;
            core.MouseDown += Core_MouseDown;
            core.MouseUp += Core_MouseUp;
            core.MouseMove += Core_MouseMove;
            core.MouseWheel += Core_MouseWheel;
        }

        private static void Core_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Shift || e.Key.IsShift())
                shift = true;
            if (e.Control || e.Key.IsCtrl())
                ctrl = true;
            if (e.Alt || e.Key.IsAlt())
                alt = true;

            switch (e.Key)
            {
                case Key.Back:
                    InputProcessor?.KeyTyped(0, '\b');
                    break;
                default:
                    InputProcessor?.KeyDown((int)e.Key);
                    break;
            }
        }

        private static void Core_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Shift || e.Key.IsShift())
                shift = false;
            if (e.Control || e.Key.IsCtrl())
                ctrl = false;
            if (e.Alt || e.Key.IsAlt())
                alt = false;

            InputProcessor?.KeyUp((int)e.Key);
        }

        private static void Core_KeyPress(object sender, KeyPressEventArgs e)
        {
            InputProcessor?.KeyTyped(0, e.Char);
        }

        private static void Core_MouseDown(object sender, MouseButtonEventArgs e)
        {
            InputProcessor?.TouchDown(e.X, e.Y, 0, (int)e.Button);
        }

        private static void Core_MouseUp(object sender, MouseButtonEventArgs e)
        {
            InputProcessor?.TouchUp(e.X, e.Y, 0, (int)e.Button);
        }

        private static void Core_MouseMove(object sender, MouseMoveEventArgs e)
        {
            if (OpenTK.Input.Mouse.GetState().LeftButton == OpenTK.Input.ButtonState.Pressed)
            {
                InputProcessor?.TouchDragged(e.X, e.Y, 0);
            }
            else
                InputProcessor?.MouseMoved(e.X, e.Y);
        }

        private static void Core_MouseWheel(object sender, ScrollEventArgs e)
        {
            InputProcessor?.Scrolled(e.X - previousWheelX, e.Y - previousWheelY);
            previousWheelX = e.X;
            previousWheelY = e.Y;
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
    }
}
