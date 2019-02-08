using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry
{
    public enum Key : int
    {
        /// <summary>A key outside the known Key.</summary>
        Unknown = 0,

        // Modifiers
        /// <summary>The left shift key.</summary>
        ShiftLeft,
        /// <summary>The left shift key (equivalent to ShiftLeft).</summary>
        LShift = ShiftLeft,
        /// <summary>The right shift key.</summary>
        ShiftRight,
        /// <summary>The right shift key (equivalent to ShiftRight).</summary>
        RShift = ShiftRight,
        /// <summary>The left control key.</summary>
        ControlLeft,
        /// <summary>The left control key (equivalent to ControlLeft).</summary>
        LControl = ControlLeft,
        /// <summary>The right control key.</summary>
        ControlRight,
        /// <summary>The right control key (equivalent to ControlRight).</summary>
        RControl = ControlRight,
        /// <summary>The left alt key.</summary>
        AltLeft,
        /// <summary>The left alt key (equivalent to AltLeft.</summary>
        LAlt = AltLeft,
        /// <summary>The right alt key.</summary>
        AltRight,
        /// <summary>The right alt key (equivalent to AltRight).</summary>
        RAlt = AltRight,
        /// <summary>The left win key.</summary>
        WinLeft,
        /// <summary>The left win key (equivalent to WinLeft).</summary>
        LWin = WinLeft,
        /// <summary>The right win key.</summary>
        WinRight,
        /// <summary>The right win key (equivalent to WinRight).</summary>
        RWin = WinRight,
        /// <summary>The menu key.</summary>
        Menu,

        // Function keys (hopefully enough for most keyboards - mine has 26)
        // <keysymdef.h> on X11 reports up to 35 function Key.
        /// <summary>The F1 key.</summary>
        F1,
        /// <summary>The F2 key.</summary>
        F2,
        /// <summary>The F3 key.</summary>
        F3,
        /// <summary>The F4 key.</summary>
        F4,
        /// <summary>The F5 key.</summary>
        F5,
        /// <summary>The F6 key.</summary>
        F6,
        /// <summary>The F7 key.</summary>
        F7,
        /// <summary>The F8 key.</summary>
        F8,
        /// <summary>The F9 key.</summary>
        F9,
        /// <summary>The F10 key.</summary>
        F10,
        /// <summary>The F11 key.</summary>
        F11,
        /// <summary>The F12 key.</summary>
        F12,
        /// <summary>The F13 key.</summary>
        F13,
        /// <summary>The F14 key.</summary>
        F14,
        /// <summary>The F15 key.</summary>
        F15,
        /// <summary>The F16 key.</summary>
        F16,
        /// <summary>The F17 key.</summary>
        F17,
        /// <summary>The F18 key.</summary>
        F18,
        /// <summary>The F19 key.</summary>
        F19,
        /// <summary>The F20 key.</summary>
        F20,
        /// <summary>The F21 key.</summary>
        F21,
        /// <summary>The F22 key.</summary>
        F22,
        /// <summary>The F23 key.</summary>
        F23,
        /// <summary>The F24 key.</summary>
        F24,
        /// <summary>The F25 key.</summary>
        F25,
        /// <summary>The F26 key.</summary>
        F26,
        /// <summary>The F27 key.</summary>
        F27,
        /// <summary>The F28 key.</summary>
        F28,
        /// <summary>The F29 key.</summary>
        F29,
        /// <summary>The F30 key.</summary>
        F30,
        /// <summary>The F31 key.</summary>
        F31,
        /// <summary>The F32 key.</summary>
        F32,
        /// <summary>The F33 key.</summary>
        F33,
        /// <summary>The F34 key.</summary>
        F34,
        /// <summary>The F35 key.</summary>
        F35,

        // Direction arrows
        /// <summary>The up arrow key.</summary>
        Up,
        /// <summary>The down arrow key.</summary>
        Down,
        /// <summary>The left arrow key.</summary>
        Left,
        /// <summary>The right arrow key.</summary>
        Right,

        /// <summary>The enter key.</summary>
        Enter,
        /// <summary>The escape key.</summary>
        Escape,
        /// <summary>The space key.</summary>
        Space,
        /// <summary>The tab key.</summary>
        Tab,
        /// <summary>The backspace key.</summary>
        BackSpace,
        /// <summary>The backspace key (equivalent to BackSpace).</summary>
        Back = BackSpace,
        /// <summary>The insert key.</summary>
        Insert,
        /// <summary>The delete key.</summary>
        Delete,
        /// <summary>The page up key.</summary>
        PageUp,
        /// <summary>The page down key.</summary>
        PageDown,
        /// <summary>The home key.</summary>
        Home,
        /// <summary>The end key.</summary>
        End,
        /// <summary>The caps lock key.</summary>
        CapsLock,
        /// <summary>The scroll lock key.</summary>
        ScrollLock,
        /// <summary>The print screen key.</summary>
        PrintScreen,
        /// <summary>The pause key.</summary>
        Pause,
        /// <summary>The num lock key.</summary>
        NumLock,

        // Special keys
        /// <summary>The clear key (Keypad5 with NumLock disabled, on typical keyboards).</summary>
        Clear,
        /// <summary>The sleep key.</summary>
        Sleep,
        /*LogOff,
        Help,
        Undo,
        Redo,
        New,
        Open,
        Close,
        Reply,
        Forward,
        Send,
        Spell,
        Save,
        Calculator,
        
        // Folders and applications
        Documents,
        Pictures,
        Music,
        MediaPlayer,
        Mail,
        Browser,
        Messenger,
        
        // Multimedia keys
        Mute,
        PlayPause,
        Stop,
        VolumeUp,
        VolumeDown,
        TrackPrevious,
        TrackNext,*/

        // Keypad keys
        /// <summary>The keypad 0 key.</summary>
        Keypad0,
        /// <summary>The keypad 1 key.</summary>
        Keypad1,
        /// <summary>The keypad 2 key.</summary>
        Keypad2,
        /// <summary>The keypad 3 key.</summary>
        Keypad3,
        /// <summary>The keypad 4 key.</summary>
        Keypad4,
        /// <summary>The keypad 5 key.</summary>
        Keypad5,
        /// <summary>The keypad 6 key.</summary>
        Keypad6,
        /// <summary>The keypad 7 key.</summary>
        Keypad7,
        /// <summary>The keypad 8 key.</summary>
        Keypad8,
        /// <summary>The keypad 9 key.</summary>
        Keypad9,
        /// <summary>The keypad divide key.</summary>
        KeypadDivide,
        /// <summary>The keypad multiply key.</summary>
        KeypadMultiply,
        /// <summary>The keypad subtract key.</summary>
        KeypadSubtract,
        /// <summary>The keypad minus key (equivalent to KeypadSubtract).</summary>
        KeypadMinus = KeypadSubtract,
        /// <summary>The keypad add key.</summary>
        KeypadAdd,
        /// <summary>The keypad plus key (equivalent to KeypadAdd).</summary>
        KeypadPlus = KeypadAdd,
        /// <summary>The keypad decimal key.</summary>
        KeypadDecimal,
        /// <summary>The keypad enter key.</summary>
        KeypadEnter,

        // Letters
        /// <summary>The A key.</summary>
        A,
        /// <summary>The B key.</summary>
        B,
        /// <summary>The C key.</summary>
        C,
        /// <summary>The D key.</summary>
        D,
        /// <summary>The E key.</summary>
        E,
        /// <summary>The F key.</summary>
        F,
        /// <summary>The G key.</summary>
        G,
        /// <summary>The H key.</summary>
        H,
        /// <summary>The I key.</summary>
        I,
        /// <summary>The J key.</summary>
        J,
        /// <summary>The K key.</summary>
        K,
        /// <summary>The L key.</summary>
        L,
        /// <summary>The M key.</summary>
        M,
        /// <summary>The N key.</summary>
        N,
        /// <summary>The O key.</summary>
        O,
        /// <summary>The P key.</summary>
        P,
        /// <summary>The Q key.</summary>
        Q,
        /// <summary>The R key.</summary>
        R,
        /// <summary>The S key.</summary>
        S,
        /// <summary>The T key.</summary>
        T,
        /// <summary>The U key.</summary>
        U,
        /// <summary>The V key.</summary>
        V,
        /// <summary>The W key.</summary>
        W,
        /// <summary>The X key.</summary>
        X,
        /// <summary>The Y key.</summary>
        Y,
        /// <summary>The Z key.</summary>
        Z,

        // Numbers
        /// <summary>The number 0 key.</summary>
        Number0,
        /// <summary>The number 1 key.</summary>
        Number1,
        /// <summary>The number 2 key.</summary>
        Number2,
        /// <summary>The number 3 key.</summary>
        Number3,
        /// <summary>The number 4 key.</summary>
        Number4,
        /// <summary>The number 5 key.</summary>
        Number5,
        /// <summary>The number 6 key.</summary>
        Number6,
        /// <summary>The number 7 key.</summary>
        Number7,
        /// <summary>The number 8 key.</summary>
        Number8,
        /// <summary>The number 9 key.</summary>
        Number9,

        // Symbols
        /// <summary>The tilde key.</summary>
        Tilde,
        /// <summary>The minus key.</summary>
        Minus,
        //Equal,
        /// <summary>The plus key.</summary>
        Plus,
        /// <summary>The left bracket key.</summary>
        BracketLeft,
        /// <summary>The left bracket key (equivalent to BracketLeft).</summary>
        LBracket = BracketLeft,
        /// <summary>The right bracket key.</summary>
        BracketRight,
        /// <summary>The right bracket key (equivalent to BracketRight).</summary>
        RBracket = BracketRight,
        /// <summary>The semicolon key.</summary>
        Semicolon,
        /// <summary>The quote key.</summary>
        Quote,
        /// <summary>The comma key.</summary>
        Comma,
        /// <summary>The period key.</summary>
        Period,
        /// <summary>The slash key.</summary>
        Slash,
        /// <summary>The backslash key.</summary>
        BackSlash,
        /// <summary>Indicates the last available keyboard key.</summary>
        LastKey
    }

    public static class KeyExt
    {
        public static char? GetChar(this Key key)
        {
            var isShiftDown = Input.IsShiftDown();

            if (key == Key.A)
                return isShiftDown ? 'A' : 'a';

            if (key == Key.B)
                return isShiftDown ? 'B' : 'b';

            if (key == Key.C)
                return isShiftDown ? 'C' : 'c';

            if (key == Key.D)
                return isShiftDown ? 'D' : 'd';

            if (key == Key.E)
                return isShiftDown ? 'E' : 'e';

            if (key == Key.F)
                return isShiftDown ? 'F' : 'f';

            if (key == Key.G)
                return isShiftDown ? 'G' : 'g';

            if (key == Key.H)
                return isShiftDown ? 'H' : 'h';

            if (key == Key.I)
                return isShiftDown ? 'I' : 'i';

            if (key == Key.J)
                return isShiftDown ? 'J' : 'j';

            if (key == Key.K)
                return isShiftDown ? 'K' : 'k';

            if (key == Key.L)
                return isShiftDown ? 'L' : 'l';

            if (key == Key.M)
                return isShiftDown ? 'M' : 'm';

            if (key == Key.N)
                return isShiftDown ? 'N' : 'n';

            if (key == Key.O)
                return isShiftDown ? 'O' : 'o';

            if (key == Key.P)
                return isShiftDown ? 'P' : 'p';

            if (key == Key.Q)
                return isShiftDown ? 'Q' : 'q';

            if (key == Key.R)
                return isShiftDown ? 'R' : 'r';

            if (key == Key.S)
                return isShiftDown ? 'S' : 's';

            if (key == Key.T)
                return isShiftDown ? 'T' : 't';

            if (key == Key.U)
                return isShiftDown ? 'U' : 'u';

            if (key == Key.V)
                return isShiftDown ? 'V' : 'v';

            if (key == Key.W)
                return isShiftDown ? 'W' : 'w';

            if (key == Key.X)
                return isShiftDown ? 'X' : 'x';

            if (key == Key.Y)
                return isShiftDown ? 'Y' : 'y';

            if (key == Key.Z)
                return isShiftDown ? 'Z' : 'z';


            /*if ((key == Key.D0 && !isShiftDown) || key == Key.NumPad0)
                return '0';

            if ((key == Key.D1 && !isShiftDown) || key == Key.NumPad1)
                return '1';

            if ((key == Key.D2 && !isShiftDown) || key == Key.NumPad2)
                return '2';

            if ((key == Key.D3 && !isShiftDown) || key == Key.NumPad3)
                return '3';

            if ((key == Key.D4 && !isShiftDown) || key == Key.NumPad4)
                return '4';

            if ((key == Key.D5 && !isShiftDown) || key == Key.NumPad5)
                return '5';

            if ((key == Key.D6 && !isShiftDown) || key == Key.NumPad6)
                return '6';

            if ((key == Key.D7 && !isShiftDown) || key == Key.NumPad7)
                return '7';

            if ((key == Key.D8 && !isShiftDown) || key == Key.NumPad8)
                return '8';

            if ((key == Key.D9 && !isShiftDown) || key == Key.NumPad9)
                return '9';


            if (key == Key.D0 && isShiftDown)
                return ')';

            if (key == Key.D1 && isShiftDown)
                return '!';

            if (key == Key.D2 && isShiftDown)
                return '@';

            if (key == Key.D3 && isShiftDown)
                return '#';

            if (key == Key.D4 && isShiftDown)
                return '$';

            if (key == Key.D5 && isShiftDown)
                return '%';

            if (key == Key.D6 && isShiftDown)
                return '^';

            if (key == Key.D7 && isShiftDown)
                return '&';

            if (key == Key.D8 && isShiftDown)
                return '*';

            if (key == Key.D9 && isShiftDown)
                return '(';*/


            if (key == Key.Space)
                return ' ';

            if (key == Key.Tab)
                return '\t';

            if (key == Key.Enter)
                return (char)13;

            if (key == Key.Back)
                return (char)8;

            if (key == Key.Delete)
                return (char)127;


            if (key == Key.KeypadAdd)
                return '+';

            if (key == Key.KeypadDecimal)
                return '.';

            if (key == Key.KeypadDivide)
                return '/';

            if (key == Key.KeypadMultiply)
                return '*';

            if (key == Key.BackSlash)
                return '\\';

            if (key == Key.Comma && !isShiftDown)
                return ',';

            if (key == Key.Comma && isShiftDown)
                return '<';

            if (key == Key.BracketLeft && !isShiftDown)
                return '[';

            if (key == Key.BracketLeft && isShiftDown)
                return '{';

            if (key == Key.BracketRight && !isShiftDown)
                return ']';

            if (key == Key.BracketRight && isShiftDown)
                return '}';

            if (key == Key.Period && !isShiftDown)
                return '.';

            if (key == Key.Period && isShiftDown)
                return '>';

            //if (key == Key.OemPipe && !isShiftDown)
            //    return '\\';

            //if (key == Key.OemPipe && isShiftDown)
            //    return '|';

            if (key == Key.Plus && !isShiftDown)
                return '=';

            if (key == Key.Plus && isShiftDown)
                return '+';

            if (key == Key.Minus && !isShiftDown)
                return '-';

            if (key == Key.Minus && isShiftDown)
                return '_';

            if (key == Key.Slash && !isShiftDown)
                return '/';

            if (key == Key.Slash && isShiftDown)
                return '?';

            /*if (key == Key.OemQuotes && !isShiftDown)
                return '\'';

            if (key == Key.OemQuotes && isShiftDown)
                return '"';

            if (key == Key.OemSemicolon && !isShiftDown)
                return ';';

            if (key == Key.OemSemicolon && isShiftDown)
                return ':';

            if (key == Key.OemTilde && !isShiftDown)
                return '`';

            if (key == Key.OemTilde && isShiftDown)
                return '~';

            if (key == Key.Subtract)
                return '-';*/

            return null;
        }
    }
}
