using Microsoft.Xna.Framework.Input;

namespace BlueberryCore
{
    public static class InputUtils
    {
        static InputUtils()
        {

        }

        public static bool IsShiftDown()
        {
            return Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.RightShift);
        }

        public static bool IsCtrlDown()
        {
            return Input.IsKeyDown(Keys.LeftControl) || Input.IsKeyDown(Keys.RightControl);
        }

        public static bool IsAltDown()
        {
            return Input.IsKeyDown(Keys.LeftAlt) || Input.IsKeyDown(Keys.RightAlt);
        }
    }
}
