using BlueberryOpenTK;

namespace Blueberry
{
    public static partial class Input
    {
        private static float previousWheelX, previousWheelY;
        private static bool shift, alt, ctrl;

        public static void Initialize(Core core)
        {
            core.KeyDown += (sender, args) =>
            {
                if (args.Shift || args.Key.IsShift())
                    shift = true;
                if (args.Control || args.Key.IsCtrl())
                    ctrl = true;
                if (args.Alt || args.Key.IsAlt())
                    alt = true;

                InputProcessor?.KeyDown((int)args.Key);
            };
            core.KeyUp += (sender, args) =>
            {
                if (args.Shift || args.Key.IsShift())
                    shift = false;
                if (args.Control || args.Key.IsCtrl())
                    ctrl = false;
                if (args.Alt || args.Key.IsAlt())
                    alt = false;

                InputProcessor?.KeyUp((int)args.Key);
            };
            core.KeyPress += (sender, args) =>
            {
                InputProcessor?.KeyTyped(0, args.KeyChar);
            };
            core.MouseMove += (sender, args) =>
            {
                if (args.Mouse.LeftButton == OpenTK.Input.ButtonState.Pressed)
                {
                    InputProcessor?.TouchDragged(args.X, args.Y, 0);
                }
                else
                    InputProcessor?.MouseMoved(args.X, args.Y);
            };
            core.MouseDown += (sender, args) =>
            {
                InputProcessor?.TouchDown(args.X, args.Y, 0, (int)args.Button);
            };
            core.MouseUp += (sender, args) =>
            {
                InputProcessor?.TouchUp(args.X, args.Y, 0, (int)args.Button);
            };
            core.MouseWheel += (sender, args) =>
            {
                InputProcessor?.Scrolled(args.Mouse.Scroll.X - previousWheelX, args.Mouse.Scroll.Y - previousWheelY);
                previousWheelX = args.Mouse.Scroll.X;
                previousWheelY = args.Mouse.Scroll.Y;
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
