
using System;
using System.Runtime.InteropServices;

namespace BlueberryCore
{
    public static class Clipboard
    {
        public static void SetText(string text)
        {
            CLRWrapper.Clipboard.SetText(text);
        }

        public static string GetText()
        {
            return CLRWrapper.Clipboard.GetText();
        }
    }
}
