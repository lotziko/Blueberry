using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry
{
    public static partial class Input
    {
        public static IInputProcessor InputProcessor { get; set; }

        public class KeyDownEventArgs : EventArgs
        {
            Key key;
        }
    }
}
