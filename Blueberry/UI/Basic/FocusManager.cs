
namespace Blueberry.UI
{
    public static class FocusManager
    {
        private static IFocusable focusedWidget;

        public static void SwitchFocus(Stage stage, IFocusable widget)
        {
            if (focusedWidget == widget) return;
            if (focusedWidget != null) focusedWidget.FocusLost();
            focusedWidget = widget;
            if (stage != null) stage.SetKeyboardFocus(null);
            focusedWidget.FocusGained();
        }

        public static void ResetFocus(Stage stage)
        {
            if (focusedWidget != null) focusedWidget.FocusLost();
            if (stage != null) stage.SetKeyboardFocus(null);
            focusedWidget = null;
        }

        public static void ResetFocus(Stage stage, Element caller)
        {
            if (focusedWidget != null) focusedWidget.FocusLost();
            if (stage != null && stage.GetKeyboardFocus() == caller) stage.SetKeyboardFocus(null);
            focusedWidget = null;
        }

        public static IFocusable GetFocusedWidget()
        {
            return focusedWidget;
        }

    }
}
