
using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BlueberryCore
{
    public static class Clipboard
    {
        //[DllImport("x64/CLRWrapper.dll")]
        //private static extern void SetTextInternal(string text);

        public static void SetText(string text)
        {
            //SetTextInternal(text);
            //CLRWrapper.Clipboard.SetText(text);
        }

        public static string GetText()
        {
            return "";
            //return CLRWrapper.Clipboard.GetText();
        }

        public static void SetImage(Color[] data, int width, int height)
        {
            var ints = new uint[width * height];
            for(int i = 0; i < data.Length; i++)
            {
                ints[i] = data[i].PackedValue;
            }
            //CLRWrapper.Clipboard.SetImage(ints, width, height);
        }

        public static void GetImage()
        {
              
        }
    }
}
