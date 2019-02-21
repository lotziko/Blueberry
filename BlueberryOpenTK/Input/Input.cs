using BlueberryOpenTK;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Blueberry
{
    public static partial class Input
    {
        private static float previousWheelX, previousWheelY;
        private static bool shift, alt, ctrl;

        #region Helper

        [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int ToUnicodeEx(
           uint wVirtKey,
           uint wScanCode,
           Keys[] lpKeyState,
           StringBuilder pwszBuff,
           int cchBuff,
           uint wFlags,
           IntPtr dwhkl);

        [DllImport("user32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetKeyboardLayout(uint threadId);

        [DllImport("user32.dll", ExactSpelling = true)]
        internal static extern bool GetKeyboardState(Keys[] keyStates);

        [DllImport("user32.dll", ExactSpelling = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr hwindow, out uint processId);

        public static string CodeToString(int scanCode)
        {
            uint procId;
            uint thread = GetWindowThreadProcessId(Process.GetCurrentProcess().MainWindowHandle, out procId);
            IntPtr hkl = GetKeyboardLayout(thread);

            if (hkl == IntPtr.Zero)
            {
                Console.WriteLine("Sorry, that keyboard does not seem to be valid.");
                return string.Empty;
            }

            Keys[] keyStates = new Keys[256];
            if (!GetKeyboardState(keyStates))
                return string.Empty;

            StringBuilder sb = new StringBuilder(10);
            int rc = ToUnicodeEx((uint)scanCode, (uint)scanCode, keyStates, sb, sb.Capacity, 0, hkl);
            return sb.ToString();
        }

        #endregion

        public static void Initialize(Core core)
        {
            core.KeyDown += On_KeyDown;
            core.KeyUp += On_KeyUp;
            core.KeyPress += On_KeyPress;
            core.MouseMove += On_MouseMove;
            core.MouseDown += On_MouseDown;
            core.MouseUp += On_MouseUp;
            core.MouseWheel += On_MouseWheel;
        }

        public static void On_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Shift || e.Key.IsShift())
                shift = true;
            if (e.Control || e.Key.IsCtrl())
                ctrl = true;
            if (e.Alt || e.Key.IsAlt())
                alt = true;

            //Console.WriteLine(CodeToString((int)e.ScanCode));

            switch (e.Key)
            {
                case OpenTK.Input.Key.Back:
                    InputProcessor?.KeyTyped(0, '\b');
                    break;
                default:
                    InputProcessor?.KeyDown((int)e.Key);
                    break;
            }
        }

        public static void On_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Shift || e.Key.IsShift())
                shift = false;
            if (e.Control || e.Key.IsCtrl())
                ctrl = false;
            if (e.Alt || e.Key.IsAlt())
                alt = false;

            InputProcessor?.KeyUp((int)e.Key);
        }

        public static void On_KeyPress(object sender, OpenTK.KeyPressEventArgs e)
        {
            InputProcessor?.KeyTyped(0, e.KeyChar);
        }

        public static void On_MouseMove(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            if (e.Mouse.LeftButton == OpenTK.Input.ButtonState.Pressed)
            {
                InputProcessor?.TouchDragged(e.X, e.Y, 0);
            }
            else
                InputProcessor?.MouseMoved(e.X, e.Y);
        }

        public static void On_MouseDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            InputProcessor?.TouchDown(e.X, e.Y, 0, (int)e.Button);
        }

        public static void On_MouseUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            InputProcessor?.TouchUp(e.X, e.Y, 0, (int)e.Button);
        }

        public static void On_MouseWheel(object sender, OpenTK.Input.MouseWheelEventArgs e)
        {
            InputProcessor?.Scrolled(e.Mouse.Scroll.X - previousWheelX, e.Mouse.Scroll.Y - previousWheelY);
            previousWheelX = e.Mouse.Scroll.X;
            previousWheelY = e.Mouse.Scroll.Y;
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
