
using Microsoft.Xna.Framework;
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

        public static void SetImage(Color[] data, int width, int height)
        {
            var ints = new uint[width * height];
            for(int i = 0; i < data.Length; i++)
            {
                ints[i] = data[i].PackedValue;
            }
            CLRWrapper.Clipboard.SetImage(ints, width, height);
        }

        public static void GetImage()
        {
              
        }
    }
}
