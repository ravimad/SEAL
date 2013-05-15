using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StaticCollections
{
    /// <summary>
    /// An enumeration of all the possible key values on a keyboard.
    /// </summary>
    enum Key : int
    {
        /// <summary>
        ///     No key pressed.
        /// </summary>
        None = 0,

        /// <summary>
        ///     The CANCEL key.
        /// </summary>
        Cancel = 0x03,

        /// <summary>
        ///     The BACKSPACE key.
        /// </summary>
        Back = 0x08,

        /// <summary>
        ///     The TAB key.
        /// </summary>
        Tab = 0x09,

        /// <summary>
        ///     The LineFeed key.
        /// </summary>
        LineFeed,

        /// <summary>
        ///     The CLEAR key.
        /// </summary>
        Clear = 0x0C,

        /// <summary>
        ///     The RETURN key.
        /// </summary>
        Return = 0x0D,

        /// <summary>
        ///     The ENTER key.
        /// </summary>
        Enter = Return,

        /// <summary>
        ///     The SHIFT key.
        /// </summary>
        Shift = 0x10,

        /// <summary>
        ///     The CTRL key.
        /// </summary>
        Ctrl = 0x11,

        /// <summary>
        ///     The ALT key.
        /// </summary>
        Alt = 0x12,

        /// <summary>
        ///     The PAUSE key.
        /// </summary>
        Pause = 0x13,

        /// <summary>
        ///     The CAPS LOCK key.
        /// </summary>
        Capital = 0x14,

        /// <summary>
        ///     The CAPS LOCK key.
        /// </summary>
        CapsLock = Capital,

        /// <summary>
        ///     The IME Kana mode key.
        /// </summary>
        KanaMode = 0x15,

        /// <summary>
        ///     The IME Hangul mode key.
        /// </summary>
        HangulMode = KanaMode,

        /// <summary>
        ///     The IME Junja mode key.
        /// </summary>
        JunjaMode = 0x17,

        /// <summary>
        ///     The IME Final mode key.
        /// </summary>
        FinalMode = 0x18,

        /// <summary>
        ///     The IME Hanja mode key.
        /// </summary>
        HanjaMode = 0x19,

        /// <summary>
        ///     The IME Kanji mode key.
        /// </summary>
        KanjiMode = HanjaMode,

        /// <summary>
        ///     The ESC key.
        /// </summary>
        Escape = 0x1B,

        /// <summary>
        ///     The IME Convert key.
        /// </summary>
        ImeConvert = 0x1C,

        /// <summary>
        ///     The IME NonConvert key.
        /// </summary>
        ImeNonConvert = 0x1D,

        /// <summary>
        ///     The IME Accept key.
        /// </summary>
        ImeAccept = 0x1E,

        /// <summary>
        ///     The IME Mode change request.
        /// </summary>
        ImeModeChange = 0x1F,

        /// <summary>
        ///     The SPACEBAR key.
        /// </summary>
        Space = 0x20,

        /// <summary>
        ///     The PAGE UP key.
        /// </summary>
        Prior = 0x21,

        /// <summary>
        ///     The PAGE UP key.
        /// </summary>
        PageUp = Prior,

        /// <summary>
        ///     The PAGE DOWN key.
        /// </summary>
        Next = 0x22,

        /// <summary>
        ///     The PAGE DOWN key.
        /// </summary>
        PageDown = Next,

        /// <summary>
        ///     The END key.
        /// </summary>
        End = 0x23,

        /// <summary>
        ///     The HOME key.
        /// </summary>
        Home = 0x24,

        /// <summary>
        ///     The LEFT ARROW key.
        /// </summary>
        Left = 0x25,

        /// <summary>
        ///     The UP ARROW key.
        /// </summary>
        Up = 0x26,

        /// <summary>
        ///     The RIGHT ARROW key.
        /// </summary>
        Right = 0x27,

        /// <summary>
        ///     The DOWN ARROW key.
        /// </summary>
        Down = 0x28,

        /// <summary>
        ///     The SELECT key.
        /// </summary>
        Select = 0x29,

        /// <summary>
        ///     The PRINT key.
        /// </summary>
        Print = 0x2A,

        /// <summary>
        ///     The EXECUTE key.
        /// </summary>
        Execute = 0x2B,

        /// <summary>
        ///     The SNAPSHOT key.
        /// </summary>
        Snapshot = 0x2C,

        /// <summary>
        ///     The PRINT SCREEN key.
        /// </summary>
        PrintScreen = Snapshot,

        /// <summary>
        ///     The INS key.
        /// </summary>
        Insert = 0x2D,

        /// <summary>
        ///     The DEL key.
        /// </summary>
        Delete = 0x2E,

        /// <summary>
        ///     The HELP key.
        /// </summary>
        Help = 0x2F,

        /// <summary>
        ///     The 0 key.
        /// </summary>
        D0 = 0x30,

        /// <summary>
        ///     The 1 key.
        /// </summary>
        D1 = 0x31,

        /// <summary>
        ///     The 2 key.
        /// </summary>
        D2 = 0x32,

        /// <summary>
        ///     The 3 key.
        /// </summary>
        D3 = 0x33,

        /// <summary>
        ///     The 4 key.
        /// </summary>
        D4 = 0x34,

        /// <summary>
        ///     The 5 key.
        /// </summary>
        D5 = 0x35,

        /// <summary>
        ///     The 6 key.
        /// </summary>
        D6 = 0x36,

        /// <summary>
        ///     The 7 key.
        /// </summary>
        D7 = 0x37,

        /// <summary>
        ///     The 8 key.
        /// </summary>
        D8 = 0x38,

        /// <summary>
        ///     The 9 key.
        /// </summary>
        D9 = 0x39,

        /// <summary>
        ///     The A key.
        /// </summary>
        A = 0x41,

        /// <summary>
        ///     The B key.
        /// </summary>
        B = 0x42,

        /// <summary>
        ///     The C key.
        /// </summary>
        C = 0x43,

        /// <summary>
        ///     The D key.
        /// </summary>
        D = 0x44,

        /// <summary>
        ///     The E key.
        /// </summary>
        E = 0x45,

        /// <summary>
        ///     The F key.
        /// </summary>
        F = 0x46,

        /// <summary>
        ///     The G key.
        /// </summary>
        G = 0x47,

        /// <summary>
        ///     The H key.
        /// </summary>
        H = 0x48,

        /// <summary>
        ///     The I key.
        /// </summary>
        I = 0x49,

        /// <summary>
        ///     The J key.
        /// </summary>
        J = 0x4A,

        /// <summary>
        ///     The K key.
        /// </summary>
        K = 0x4B,

        /// <summary>
        ///     The L key.
        /// </summary>
        L = 0x4C,

        /// <summary>
        ///     The M key.
        /// </summary>
        M = 0x4D,

        /// <summary>
        ///     The N key.
        /// </summary>
        N = 0x4E,

        /// <summary>
        ///     The O key.
        /// </summary>
        O = 0x4F,

        /// <summary>
        ///     The P key.
        /// </summary>
        P = 0x50,

        /// <summary>
        ///     The Q key.
        /// </summary>
        Q = 0x51,

        /// <summary>
        ///     The R key.
        /// </summary>
        R = 0x52,

        /// <summary>
        ///     The S key.
        /// </summary>
        S = 0x53,

        /// <summary>
        ///     The T key.
        /// </summary>
        T = 0x54,

        /// <summary>
        ///     The U key.
        /// </summary>
        U = 0x55,

        /// <summary>
        ///     The V key.
        /// </summary>
        V = 0x56,

        /// <summary>
        ///     The W key.
        /// </summary>
        W = 0x57,

        /// <summary>
        ///     The X key.
        /// </summary>
        X = 0x58,

        /// <summary>
        ///     The Y key.
        /// </summary>
        Y = 0x59,

        /// <summary>
        ///     The Z key.
        /// </summary>
        Z = 0x5A,

        /// <summary>
        ///     The left Windows logo key (Microsoft Natural Keyboard).
        /// </summary>
        LWin = 0x5B,

        /// <summary>
        ///     The right Windows logo key (Microsoft Natural Keyboard).
        /// </summary>
        RWin = 0x5C,

        /// <summary>
        ///     The Application key (Microsoft Natural Keyboard).
        /// </summary>
        Apps = 0x5D,

        /// <summary>
        ///     The Computer Sleep key.
        /// </summary>
        Sleep = 0x5F,

        /// <summary>
        ///     The 0 key on the numeric keypad.
        /// </summary>
        NumPad0 = 0x60,

        /// <summary>
        ///     The 1 key on the numeric keypad.
        /// </summary>
        NumPad1 = 0x61,

        /// <summary>
        ///     The 2 key on the numeric keypad.
        /// </summary>
        NumPad2 = 0x62,

        /// <summary>
        ///     The 3 key on the numeric keypad.
        /// </summary>
        NumPad3 = 0x63,

        /// <summary>
        ///     The 4 key on the numeric keypad.
        /// </summary>
        NumPad4 = 0x64,

        /// <summary>
        ///     The 5 key on the numeric keypad.
        /// </summary>
        NumPad5 = 0x65,

        /// <summary>
        ///     The 6 key on the numeric keypad.
        /// </summary>
        NumPad6 = 0x66,

        /// <summary>
        ///     The 7 key on the numeric keypad.
        /// </summary>
        NumPad7 = 0x67,

        /// <summary>
        ///     The 8 key on the numeric keypad.
        /// </summary>
        NumPad8 = 0x68,

        /// <summary>
        ///     The 9 key on the numeric keypad.
        /// </summary>
        NumPad9 = 0x69,

        /// <summary>
        ///     The Multiply key.
        /// </summary>
        Multiply = 0x6A,

        /// <summary>
        ///     The Add key.
        /// </summary>
        Add = 0x6B,

        /// <summary>
        ///     The Separator key.
        /// </summary>
        Separator = 0x6C,

        /// <summary>
        ///     The Subtract key.
        /// </summary>
        Subtract = 0x6D,

        /// <summary>
        ///     The Decimal key.
        /// </summary>
        Decimal = 0x6E,

        /// <summary>
        ///     The Divide key.
        /// </summary>
        Divide = 0x6F,

        /// <summary>
        ///     The F1 key.
        /// </summary>
        F1 = 0x70,

        /// <summary>
        ///     The F2 key.
        /// </summary>
        F2 = 0x71,

        /// <summary>
        ///     The F3 key.
        /// </summary>
        F3 = 0x72,

        /// <summary>
        ///     The F4 key.
        /// </summary>
        F4 = 0x73,

        /// <summary>
        ///     The F5 key.
        /// </summary>
        F5 = 0x74,

        /// <summary>
        ///     The F6 key.
        /// </summary>
        F6 = 0x75,

        /// <summary>
        ///     The F7 key.
        /// </summary>
        F7 = 0x76,

        /// <summary>
        ///     The F8 key.
        /// </summary>
        F8 = 0x77,

        /// <summary>
        ///     The F9 key.
        /// </summary>
        F9 = 0x78,

        /// <summary>
        ///     The F10 key.
        /// </summary>
        F10 = 0x79,

        /// <summary>
        ///     The F11 key.
        /// </summary>
        F11 = 0x7A,

        /// <summary>
        ///     The F12 key.
        /// </summary>
        F12 = 0x7B,

        /// <summary>
        ///     The F13 key.
        /// </summary>
        F13 = 0x7C,

        /// <summary>
        ///     The F14 key.
        /// </summary>
        F14 = 0x7D,

        /// <summary>
        ///     The F15 key.
        /// </summary>
        F15 = 0x7E,

        /// <summary>
        ///     The F16 key.
        /// </summary>
        F16 = 0x7F,

        /// <summary>
        ///     The F17 key.
        /// </summary>
        F17 = 0x80,

        /// <summary>
        ///     The F18 key.
        /// </summary>
        F18 = 0x81,

        /// <summary>
        ///     The F19 key.
        /// </summary>
        F19 = 0x82,

        /// <summary>
        ///     The F20 key.
        /// </summary>
        F20 = 0x83,

        /// <summary>
        ///     The F21 key.
        /// </summary>
        F21 = 0x84,

        /// <summary>
        ///     The F22 key.
        /// </summary>
        F22 = 0x85,

        /// <summary>
        ///     The F23 key.
        /// </summary>
        F23 = 0x86,

        /// <summary>
        ///     The F24 key.
        /// </summary>
        F24 = 0x87,

        /// <summary>
        ///     The NUM LOCK key.
        /// </summary>
        NumLock = 0x90,

        /// <summary>
        ///     The SCROLL LOCK key.
        /// </summary>
        Scroll = 0x91,

        /// <summary>
        ///     The left SHIFT key.
        /// </summary>
        LeftShift = 0xA0,

        /// <summary>
        ///     The right SHIFT key.
        /// </summary>
        RightShift = 0xA1,

        /// <summary>
        ///     The left CTRL key.
        /// </summary>
        LeftCtrl = 0xA2,

        /// <summary>
        ///     The right CTRL key.
        /// </summary>
        RightCtrl = 0xA3,

        /// <summary>
        ///     The left ALT key.
        /// </summary>
        LeftAlt = 0xA4,

        /// <summary>
        ///     The right ALT key.
        /// </summary>
        RightAlt = 0xA5,

        /// <summary>
        ///     The Browser Back key.
        /// </summary>
        BrowserBack = 0xA6,

        /// <summary>
        ///     The Browser Forward key.
        /// </summary>
        BrowserForward = 0xA7,

        /// <summary>
        ///     The Browser Refresh key.
        /// </summary>
        BrowserRefresh = 0xA8,

        /// <summary>
        ///     The Browser Stop key.
        /// </summary>
        BrowserStop = 0xA9,

        /// <summary>
        ///     The Browser Search key.
        /// </summary>
        BrowserSearch = 0xAA,

        /// <summary>
        ///     The Browser Favorites key.
        /// </summary>
        BrowserFavorites = 0xAB,

        /// <summary>
        ///     The Browser Home key.
        /// </summary>
        BrowserHome = 0xAC,

        /// <summary>
        ///     The Volume Mute key.
        /// </summary>
        VolumeMute = 0xAD,

        /// <summary>
        ///     The Volume Down key.
        /// </summary>
        VolumeDown = 0xAE,

        /// <summary>
        ///     The Volume Up key.
        /// </summary>
        VolumeUp = 0xAF,

        /// <summary>
        ///     The Media Next Track key.
        /// </summary>
        MediaNextTrack = 0xB0,

        /// <summary>
        ///     The Media Previous Track key.
        /// </summary>
        MediaPreviousTrack = 0xB1,

        /// <summary>
        ///     The Media Stop key.
        /// </summary>
        MediaStop = 0xB2,

        /// <summary>
        ///     The Media Play Pause key.
        /// </summary>
        MediaPlayPause = 0xB3,

        /// <summary>
        ///     The Launch Mail key.
        /// </summary>
        LaunchMail = 0xB4,

        /// <summary>
        ///     The Select Media key.
        /// </summary>
        SelectMedia = 0xB5,

        /// <summary>
        ///     The Launch Application1 key.
        /// </summary>
        LaunchApplication1 = 0xB6,

        /// <summary>
        ///     The Launch Application2 key.
        /// </summary>
        LaunchApplication2 = 0xB7,

        /// <summary>
        ///     The Oem 1 key.  ',:' for US
        /// </summary>
        Oem1 = 0xBA,

        /// <summary>
        ///     The Oem Semicolon key.
        /// </summary>
        OemSemicolon = Oem1,

        /// <summary>
        ///     The Oem plus key.  '+' any country
        /// </summary>
        OemPlus = 0xBB,

        /// <summary>
        ///     The Oem comma key.  ',' any country
        /// </summary>
        OemComma = 0xBC,

        /// <summary>
        ///     The Oem Minus key.  '-' any country
        /// </summary>
        OemMinus = 0xBD,

        /// <summary>
        ///     The Oem Period key.  '.' any country
        /// </summary>
        OemPeriod = 0xBE,

        /// <summary>
        ///     The Oem 2 key.  '/?' for US
        /// </summary>
        Oem2 = 0xBF,

        /// <summary>
        ///     The Oem Question key.
        /// </summary>
        OemQuestion = Oem2,

        /// <summary>
        ///     The Oem 3 key.  '`~' for US            
        /// </summary>
        Oem3 = 0xC0,

        /// <summary>
        ///     The Oem tilde key.
        /// </summary>
        OemTilde = Oem3,

        /// <summary>
        ///     The ABNT_C1 (Brazilian) key.
        /// </summary>
        AbntC1 = 0xC1,

        /// <summary>
        ///     The ABNT_C2 (Brazilian) key.
        /// </summary>
        AbntC2 = 0xC2,

        /// <summary>
        ///     The Oem 4 key.
        /// </summary>
        Oem4 = 0xDB,

        /// <summary>
        ///     The Oem Open Brackets key.
        /// </summary>
        OemOpenBrackets = Oem4,

        /// <summary>
        ///     The Oem 5 key.
        /// </summary>
        Oem5 = 0xDC,

        /// <summary>
        ///     The Oem Pipe key.
        /// </summary>
        OemPipe = Oem5,

        /// <summary>
        ///     The Oem 6 key.
        /// </summary>
        Oem6 = 0xDD,

        /// <summary>
        ///     The Oem Close Brackets key.
        /// </summary>
        OemCloseBrackets = Oem6,

        /// <summary>
        ///     The Oem 7 key.
        /// </summary>
        Oem7 = 0xDE,

        /// <summary>
        ///     The Oem Quotes key.
        /// </summary>
        OemQuotes = Oem7,

        /// <summary>
        ///     The Oem8 key.
        /// </summary>
        Oem8 = 0xDF,

        /// <summary>
        ///     The Oem 102 key.
        /// </summary>
        Oem102 = 0xE2,

        /// <summary>
        ///     The Oem Backslash key.
        /// </summary>
        OemBackslash = Oem102,

        /// <summary>
        ///     A special key masking the real key being processed by an IME.
        /// </summary>
        ImeProcessed = 0xE5,

        /// <summary>
        ///     A special key masking the real key being processed as a system key.
        /// </summary>
        System,

        /// <summary>
        ///     The OEM_ATTN key.
        /// </summary>
        OemAttn = 0xF0,

        /// <summary>
        ///     The DBE_ALPHANUMERIC key.
        /// </summary>
        DbeAlphanumeric = OemAttn,

        /// <summary>
        ///     The OEM_FINISH key.
        /// </summary>
        OemFinish = 0xF1,

        /// <summary>
        ///     The DBE_KATAKANA key.
        /// </summary>
        DbeKatakana = OemFinish,

        /// <summary>
        ///     The OEM_COPY key.
        /// </summary>
        OemCopy = 0xF2,

        /// <summary>
        ///     The DBE_HIRAGANA key.
        /// </summary>
        DbeHiragana = OemCopy,

        /// <summary>
        ///     The OEM_AUTO key.
        /// </summary>
        OemAuto = 0xF3,

        /// <summary>
        ///     The DBE_SBCSCHAR key.
        /// </summary>
        DbeSbcsChar = OemAuto,

        /// <summary>
        ///     The OEM_ENLW key.
        /// </summary>
        OemEnlw = 0xF4,

        /// <summary>
        ///     The DBE_DBCSCHAR key.
        /// </summary>
        DbeDbcsChar = OemEnlw,

        /// <summary>
        ///     The OEM_BACKTAB key.
        /// </summary>
        OemBackTab = 0xF5,

        /// <summary>
        ///     The DBE_ROMAN key.
        /// </summary>
        DbeRoman = OemBackTab,

        /// <summary>
        ///     The ATTN key.
        /// </summary>
        Attn = 0xF6,

        /// <summary>
        ///     The DBE_NOROMAN key.
        /// </summary>
        DbeNoRoman = Attn,

        /// <summary>
        ///     The CRSEL key.
        /// </summary>
        CrSel = 0xF7,

        /// <summary>
        ///     The DBE_ENTERWORDREGISTERMODE key.
        /// </summary>
        DbeEnterWordRegisterMode = CrSel,

        /// <summary>
        ///     The EXSEL key.
        /// </summary>
        ExSel = 0xF8,

        /// <summary>
        ///     The DBE_ENTERIMECONFIGMODE key.
        /// </summary>
        DbeEnterImeConfigureMode = ExSel,

        /// <summary>
        ///     The ERASE EOF key.
        /// </summary>
        EraseEof = 0xF9,

        /// <summary>
        ///     The DBE_FLUSHSTRING key.
        /// </summary>
        DbeFlushString = EraseEof,

        /// <summary>
        ///     The PLAY key.
        /// </summary>
        Play = 0xFA,

        /// <summary>
        ///     The DBE_CODEINPUT key.
        /// </summary>
        DbeCodeInput = Play,

        /// <summary>
        ///     The ZOOM key.
        /// </summary>
        Zoom = 0xFB,

        /// <summary>
        ///     The DBE_NOCODEINPUT key.
        /// </summary>
        DbeNoCodeInput = Zoom,

        /// <summary>
        ///     A constant reserved for future use.
        /// </summary>
        NoName = 0xFC,

        /// <summary>
        ///     The DBE_DETERMINESTRING key.
        /// </summary>
        DbeDetermineString = NoName,

        /// <summary>
        ///     The PA1 key.
        /// </summary>
        Pa1 = 0xFD,

        /// <summary>
        ///     The DBE_ENTERDLGCONVERSIONMODE key.
        /// </summary>
        DbeEnterDialogConversionMode = Pa1,

        /// <summary>
        ///     The CLEAR key.
        /// </summary>
        OemClear = 0xFE,

        /// <summary>
        ///  Indicates the key is part of a dead-key composition
        /// </summary>
        DeadCharProcessed = 0,
    }

    struct KeySpec
    {
        public ushort KeyCode;
        public bool IsExtended;
        public string Name;

        public KeySpec(ushort keyCode, bool isExtended, string name)
        {
            this.KeyCode = keyCode;
            this.IsExtended = isExtended;
            this.Name = name;
        }
    }

    public class StaticCollections
    {
        private static Dictionary<Key, KeySpec> KeyBoardKeys = null;
        static StaticCollections()
        {
            KeyBoardKeys = new Dictionary<Key, KeySpec>();
            KeyBoardKeys.Add(Key.Cancel, new KeySpec((ushort)Key.Cancel, false, "cancel"));
            KeyBoardKeys.Add(Key.Back, new KeySpec((ushort)Key.Back, false, "backspace"));
            KeyBoardKeys.Add(Key.Tab, new KeySpec((ushort)Key.Tab, false, "tab"));
            KeyBoardKeys.Add(Key.Clear, new KeySpec((ushort)Key.Clear, false, "clear"));
            KeyBoardKeys.Add(Key.Return, new KeySpec((ushort)Key.Return, false, "return"));            
            KeyBoardKeys.Add(Key.Shift, new KeySpec((ushort)Key.Shift, false, "shift"));
            KeyBoardKeys.Add(Key.Ctrl, new KeySpec((ushort)Key.Ctrl, false, "ctrl"));
            KeyBoardKeys.Add(Key.Alt, new KeySpec((ushort)Key.Alt, false, "alt"));
            KeyBoardKeys.Add(Key.Pause, new KeySpec((ushort)Key.Pause, false, "pause"));
            KeyBoardKeys.Add(Key.Capital, new KeySpec((ushort)Key.Capital, false, "capital"));
            KeyBoardKeys.Add(Key.KanaMode, new KeySpec((ushort)Key.KanaMode, false, "kanamode"));
            KeyBoardKeys.Add(Key.JunjaMode, new KeySpec((ushort)Key.JunjaMode, false, "junjamode"));
            KeyBoardKeys.Add(Key.FinalMode, new KeySpec((ushort)Key.FinalMode, false, "finalmode"));
            KeyBoardKeys.Add(Key.HanjaMode, new KeySpec((ushort)Key.HanjaMode, false, "hanjamode"));           
            KeyBoardKeys.Add(Key.Escape, new KeySpec((ushort)Key.Escape, false, "esc"));
            KeyBoardKeys.Add(Key.ImeConvert, new KeySpec((ushort)Key.ImeConvert, false, "imeconvert"));
            KeyBoardKeys.Add(Key.ImeNonConvert, new KeySpec((ushort)Key.ImeNonConvert, false, "imenonconvert"));
            KeyBoardKeys.Add(Key.ImeAccept, new KeySpec((ushort)Key.ImeAccept, false, "imeaccept"));
            KeyBoardKeys.Add(Key.ImeModeChange, new KeySpec((ushort)Key.ImeAccept, false, "imemodechange"));
            KeyBoardKeys.Add(Key.Space, new KeySpec((ushort)Key.Space, false, " "));
            KeyBoardKeys.Add(Key.Prior, new KeySpec((ushort)Key.Prior, true, "prior"));            
            KeyBoardKeys.Add(Key.Next, new KeySpec((ushort)Key.Next, true, "next"));
            KeyBoardKeys.Add(Key.End, new KeySpec((ushort)Key.End, true, "end"));
            KeyBoardKeys.Add(Key.Home, new KeySpec((ushort)Key.Home, true, "home"));
            KeyBoardKeys.Add(Key.Left, new KeySpec((ushort)Key.Left, true, "left"));
            KeyBoardKeys.Add(Key.Up, new KeySpec((ushort)Key.Up, true, "up"));
            KeyBoardKeys.Add(Key.Right, new KeySpec((ushort)Key.Right, true, "right"));
            KeyBoardKeys.Add(Key.Down, new KeySpec((ushort)Key.Down, true, "down"));
            KeyBoardKeys.Add(Key.Select, new KeySpec((ushort)Key.Select, false, "select"));
            KeyBoardKeys.Add(Key.Print, new KeySpec((ushort)Key.Print, false, "print"));
            KeyBoardKeys.Add(Key.Execute, new KeySpec((ushort)Key.Execute, false, "execute"));
            KeyBoardKeys.Add(Key.Snapshot, new KeySpec((ushort)Key.Snapshot, true, "snapshot"));           
            KeyBoardKeys.Add(Key.Insert, new KeySpec((ushort)Key.Insert, true, "insert"));
            KeyBoardKeys.Add(Key.Delete, new KeySpec((ushort)Key.Delete, true, "delete"));
            KeyBoardKeys.Add(Key.Help, new KeySpec((ushort)Key.Help, false, "help"));

            KeyBoardKeys.Add(Key.D0, new KeySpec((ushort)Key.D0, false, "0"));
            KeyBoardKeys.Add(Key.D1, new KeySpec((ushort)Key.D1, false, "1"));
            KeyBoardKeys.Add(Key.D2, new KeySpec((ushort)Key.D2, false, "2"));
            KeyBoardKeys.Add(Key.D3, new KeySpec((ushort)Key.D3, false, "3"));
            KeyBoardKeys.Add(Key.D4, new KeySpec((ushort)Key.D4, false, "4"));
            KeyBoardKeys.Add(Key.D5, new KeySpec((ushort)Key.D5, false, "5"));
            KeyBoardKeys.Add(Key.D6, new KeySpec((ushort)Key.D6, false, "6"));
            KeyBoardKeys.Add(Key.D7, new KeySpec((ushort)Key.D7, false, "7"));
            KeyBoardKeys.Add(Key.D8, new KeySpec((ushort)Key.D8, false, "8"));
            KeyBoardKeys.Add(Key.D9, new KeySpec((ushort)Key.D9, false, "9"));

            KeyBoardKeys.Add(Key.A, new KeySpec((ushort)Key.A, false, "a"));
            KeyBoardKeys.Add(Key.B, new KeySpec((ushort)Key.B, false, "b"));
            KeyBoardKeys.Add(Key.C, new KeySpec((ushort)Key.C, false, "c"));
            KeyBoardKeys.Add(Key.D, new KeySpec((ushort)Key.D, false, "d"));
            KeyBoardKeys.Add(Key.E, new KeySpec((ushort)Key.E, false, "e"));
            KeyBoardKeys.Add(Key.F, new KeySpec((ushort)Key.F, false, "f"));
            KeyBoardKeys.Add(Key.G, new KeySpec((ushort)Key.G, false, "g"));
            KeyBoardKeys.Add(Key.H, new KeySpec((ushort)Key.H, false, "h"));
            KeyBoardKeys.Add(Key.I, new KeySpec((ushort)Key.I, false, "i"));
            KeyBoardKeys.Add(Key.J, new KeySpec((ushort)Key.J, false, "j"));
            KeyBoardKeys.Add(Key.K, new KeySpec((ushort)Key.K, false, "k"));
            KeyBoardKeys.Add(Key.L, new KeySpec((ushort)Key.L, false, "l"));
            KeyBoardKeys.Add(Key.M, new KeySpec((ushort)Key.M, false, "m"));
            KeyBoardKeys.Add(Key.N, new KeySpec((ushort)Key.N, false, "n"));
            KeyBoardKeys.Add(Key.O, new KeySpec((ushort)Key.O, false, "o"));
            KeyBoardKeys.Add(Key.P, new KeySpec((ushort)Key.P, false, "p"));
            KeyBoardKeys.Add(Key.Q, new KeySpec((ushort)Key.Q, false, "q"));
            KeyBoardKeys.Add(Key.R, new KeySpec((ushort)Key.R, false, "r"));
            KeyBoardKeys.Add(Key.S, new KeySpec((ushort)Key.S, false, "s"));
            KeyBoardKeys.Add(Key.T, new KeySpec((ushort)Key.T, false, "t"));
            KeyBoardKeys.Add(Key.U, new KeySpec((ushort)Key.U, false, "u"));
            KeyBoardKeys.Add(Key.V, new KeySpec((ushort)Key.V, false, "v"));
            KeyBoardKeys.Add(Key.W, new KeySpec((ushort)Key.W, false, "w"));
            KeyBoardKeys.Add(Key.X, new KeySpec((ushort)Key.X, false, "x"));
            KeyBoardKeys.Add(Key.Y, new KeySpec((ushort)Key.Y, false, "y"));
            KeyBoardKeys.Add(Key.Z, new KeySpec((ushort)Key.Z, false, "z"));

            KeyBoardKeys.Add(Key.LWin, new KeySpec((ushort)Key.LWin, true, "lwin"));
            KeyBoardKeys.Add(Key.RWin, new KeySpec((ushort)Key.RWin, true, "rwin"));
            KeyBoardKeys.Add(Key.Apps, new KeySpec((ushort)Key.Apps, true, "apps"));
            KeyBoardKeys.Add(Key.Sleep, new KeySpec((ushort)Key.Sleep, false, "sleep"));

            KeyBoardKeys.Add(Key.NumPad0, new KeySpec((ushort)Key.NumPad0, false, "n0"));
            KeyBoardKeys.Add(Key.NumPad1, new KeySpec((ushort)Key.NumPad1, false, "n1"));
            KeyBoardKeys.Add(Key.NumPad2, new KeySpec((ushort)Key.NumPad2, false, "n2"));
            KeyBoardKeys.Add(Key.NumPad3, new KeySpec((ushort)Key.NumPad3, false, "n3"));
            KeyBoardKeys.Add(Key.NumPad4, new KeySpec((ushort)Key.NumPad4, false, "n4"));
            KeyBoardKeys.Add(Key.NumPad5, new KeySpec((ushort)Key.NumPad5, false, "n5"));
            KeyBoardKeys.Add(Key.NumPad6, new KeySpec((ushort)Key.NumPad6, false, "n6"));
            KeyBoardKeys.Add(Key.NumPad7, new KeySpec((ushort)Key.NumPad7, false, "n7"));
            KeyBoardKeys.Add(Key.NumPad8, new KeySpec((ushort)Key.NumPad8, false, "n8"));
            KeyBoardKeys.Add(Key.NumPad9, new KeySpec((ushort)Key.NumPad9, false, "n9"));

            KeyBoardKeys.Add(Key.Multiply, new KeySpec((ushort)Key.Multiply, false, "*"));
            KeyBoardKeys.Add(Key.Add, new KeySpec((ushort)Key.Add, false, "+"));
            KeyBoardKeys.Add(Key.Separator, new KeySpec((ushort)Key.Separator, false, "separator"));
            KeyBoardKeys.Add(Key.Subtract, new KeySpec((ushort)Key.Subtract, false, "-"));
            KeyBoardKeys.Add(Key.Decimal, new KeySpec((ushort)Key.Decimal, false, "decimal"));
            KeyBoardKeys.Add(Key.Divide, new KeySpec((ushort)Key.Divide, true, "/"));

            KeyBoardKeys.Add(Key.F1, new KeySpec((ushort)Key.F1, false, "f1"));
            KeyBoardKeys.Add(Key.F2, new KeySpec((ushort)Key.F2, false, "f2"));
            KeyBoardKeys.Add(Key.F3, new KeySpec((ushort)Key.F3, false, "f3"));
            KeyBoardKeys.Add(Key.F4, new KeySpec((ushort)Key.F4, false, "f4"));
            KeyBoardKeys.Add(Key.F5, new KeySpec((ushort)Key.F5, false, "f5"));
            KeyBoardKeys.Add(Key.F6, new KeySpec((ushort)Key.F6, false, "f6"));
            KeyBoardKeys.Add(Key.F7, new KeySpec((ushort)Key.F7, false, "f7"));
            KeyBoardKeys.Add(Key.F8, new KeySpec((ushort)Key.F8, false, "f8"));
            KeyBoardKeys.Add(Key.F9, new KeySpec((ushort)Key.F9, false, "f9"));
            KeyBoardKeys.Add(Key.F10, new KeySpec((ushort)Key.F10, false, "f10"));
            KeyBoardKeys.Add(Key.F11, new KeySpec((ushort)Key.F11, false, "f11"));
            KeyBoardKeys.Add(Key.F12, new KeySpec((ushort)Key.F12, false, "f12"));
            KeyBoardKeys.Add(Key.F13, new KeySpec((ushort)Key.F13, false, "f13"));
            KeyBoardKeys.Add(Key.F14, new KeySpec((ushort)Key.F14, false, "f14"));
            KeyBoardKeys.Add(Key.F15, new KeySpec((ushort)Key.F15, false, "f15"));
            KeyBoardKeys.Add(Key.F16, new KeySpec((ushort)Key.F16, false, "f16"));
            KeyBoardKeys.Add(Key.F17, new KeySpec((ushort)Key.F17, false, "f17"));
            KeyBoardKeys.Add(Key.F18, new KeySpec((ushort)Key.F18, false, "f18"));
            KeyBoardKeys.Add(Key.F19, new KeySpec((ushort)Key.F19, false, "f19"));
            KeyBoardKeys.Add(Key.F20, new KeySpec((ushort)Key.F20, false, "f20"));
            KeyBoardKeys.Add(Key.F21, new KeySpec((ushort)Key.F21, false, "f21"));
            KeyBoardKeys.Add(Key.F22, new KeySpec((ushort)Key.F22, false, "f22"));
            KeyBoardKeys.Add(Key.F23, new KeySpec((ushort)Key.F23, false, "f23"));
            KeyBoardKeys.Add(Key.F24, new KeySpec((ushort)Key.F24, false, "f24"));

            KeyBoardKeys.Add(Key.NumLock, new KeySpec((ushort)Key.NumLock, true, "numlock"));
            KeyBoardKeys.Add(Key.Scroll, new KeySpec((ushort)Key.Scroll, false, "scroll"));
            KeyBoardKeys.Add(Key.LeftShift, new KeySpec((ushort)Key.LeftShift, false, "leftshift"));
            KeyBoardKeys.Add(Key.RightShift, new KeySpec((ushort)Key.RightShift, false, "rightshift"));
            KeyBoardKeys.Add(Key.LeftCtrl, new KeySpec((ushort)Key.LeftCtrl, false, "leftctrl"));
            KeyBoardKeys.Add(Key.RightCtrl, new KeySpec((ushort)Key.RightCtrl, true, "rightctrl"));
            KeyBoardKeys.Add(Key.LeftAlt, new KeySpec((ushort)Key.LeftAlt, false, "leftalt"));
            KeyBoardKeys.Add(Key.RightAlt, new KeySpec((ushort)Key.RightAlt, true, "rightalt"));

            KeyBoardKeys.Add(Key.BrowserBack, new KeySpec((ushort)Key.BrowserBack, false, "browserback"));
            KeyBoardKeys.Add(Key.BrowserForward, new KeySpec((ushort)Key.BrowserForward, false, "browserforward"));
            KeyBoardKeys.Add(Key.BrowserRefresh, new KeySpec((ushort)Key.BrowserRefresh, false, "browserrefresh"));
            KeyBoardKeys.Add(Key.BrowserStop, new KeySpec((ushort)Key.BrowserStop, false, "browserstop"));
            KeyBoardKeys.Add(Key.BrowserSearch, new KeySpec((ushort)Key.BrowserSearch, false, "browsersearch"));
            KeyBoardKeys.Add(Key.BrowserFavorites, new KeySpec((ushort)Key.BrowserFavorites, false, "BrowserFavorites"));
            KeyBoardKeys.Add(Key.BrowserHome, new KeySpec((ushort)Key.BrowserHome, false, "BrowserHome"));

            KeyBoardKeys.Add(Key.VolumeMute, new KeySpec((ushort)Key.VolumeMute, false, "VolumeMute"));
            KeyBoardKeys.Add(Key.VolumeDown, new KeySpec((ushort)Key.VolumeDown, false, "VolumeDown"));
            KeyBoardKeys.Add(Key.VolumeUp, new KeySpec((ushort)Key.VolumeUp, false, "VolumeUp"));
            KeyBoardKeys.Add(Key.MediaNextTrack, new KeySpec((ushort)Key.MediaNextTrack, false, "MediaNextTrack"));
            KeyBoardKeys.Add(Key.MediaPreviousTrack, new KeySpec((ushort)Key.MediaPreviousTrack, false, "MediaPreviousTrack"));
            KeyBoardKeys.Add(Key.MediaStop, new KeySpec((ushort)Key.MediaStop, false, "MediaStop"));
            KeyBoardKeys.Add(Key.MediaPlayPause, new KeySpec((ushort)Key.MediaPlayPause, false, "MediaPlayPause"));
            KeyBoardKeys.Add(Key.LaunchMail, new KeySpec((ushort)Key.LaunchMail, false, "LaunchMail"));
            KeyBoardKeys.Add(Key.SelectMedia, new KeySpec((ushort)Key.SelectMedia, false, "SelectMedia"));
            KeyBoardKeys.Add(Key.LaunchApplication1, new KeySpec((ushort)Key.LaunchApplication1, false, "LaunchApplication1"));
            KeyBoardKeys.Add(Key.LaunchApplication2, new KeySpec((ushort)Key.LaunchApplication2, false, "LaunchApplication2"));
            
            KeyBoardKeys.Add(Key.Oem1, new KeySpec((ushort)Key.Oem1, false, ";"));           
            KeyBoardKeys.Add(Key.OemPlus, new KeySpec((ushort)Key.OemPlus, false, "+"));
            KeyBoardKeys.Add(Key.OemComma, new KeySpec((ushort)Key.OemComma, false, ","));
            KeyBoardKeys.Add(Key.OemMinus, new KeySpec((ushort)Key.OemMinus, false, "-"));
            KeyBoardKeys.Add(Key.OemPeriod, new KeySpec((ushort)Key.OemPeriod, false, "."));
            KeyBoardKeys.Add(Key.Oem2, new KeySpec((ushort)Key.Oem2, false, "?"));
            KeyBoardKeys.Add(Key.Oem3, new KeySpec((ushort)Key.Oem3, false, "~"));
            KeyBoardKeys.Add(Key.AbntC1, new KeySpec((ushort)Key.AbntC1, false, "AbntC1"));
            KeyBoardKeys.Add(Key.AbntC2, new KeySpec((ushort)Key.AbntC2, false, "AbntC2"));
            KeyBoardKeys.Add(Key.Oem4, new KeySpec((ushort)Key.Oem4, false, "["));
            KeyBoardKeys.Add(Key.Oem5, new KeySpec((ushort)Key.Oem5, false, "|"));
            KeyBoardKeys.Add(Key.Oem6, new KeySpec((ushort)Key.Oem6, false, "]"));
            KeyBoardKeys.Add(Key.Oem7, new KeySpec((ushort)Key.Oem7, false, "\""));
            KeyBoardKeys.Add(Key.Oem8, new KeySpec((ushort)Key.Oem8, false, "Oem8"));
            KeyBoardKeys.Add(Key.Oem102, new KeySpec((ushort)Key.Oem102, false, "\\"));
            KeyBoardKeys.Add(Key.ImeProcessed, new KeySpec((ushort)Key.ImeProcessed, false, "ImeProcessed"));
            KeyBoardKeys.Add(Key.OemAttn, new KeySpec((ushort)Key.OemAttn, false, "OemAttn"));
            KeyBoardKeys.Add(Key.OemFinish, new KeySpec((ushort)Key.OemFinish, false, "OemFinish"));
            KeyBoardKeys.Add(Key.OemCopy, new KeySpec((ushort)Key.OemCopy, false, "OemCopy"));
            KeyBoardKeys.Add(Key.OemAuto, new KeySpec((ushort)Key.OemAuto, false, "OemAuto"));
            KeyBoardKeys.Add(Key.OemEnlw, new KeySpec((ushort)Key.OemEnlw, false, "OemEnlw"));          
            KeyBoardKeys.Add(Key.OemBackTab, new KeySpec((ushort)Key.OemBackTab, false, "OemBackTab"));
            KeyBoardKeys.Add(Key.Attn, new KeySpec((ushort)Key.Attn, false, "Attn"));
            KeyBoardKeys.Add(Key.CrSel, new KeySpec((ushort)Key.CrSel, false, "CrSel"));
            KeyBoardKeys.Add(Key.ExSel, new KeySpec((ushort)Key.ExSel, false, "ExSel"));
            KeyBoardKeys.Add(Key.EraseEof, new KeySpec((ushort)Key.EraseEof, false, "EraseEof"));
            KeyBoardKeys.Add(Key.Play, new KeySpec((ushort)Key.Play, false, "Play"));
            KeyBoardKeys.Add(Key.Zoom, new KeySpec((ushort)Key.Zoom, false, "Zoom"));
            KeyBoardKeys.Add(Key.NoName, new KeySpec((ushort)Key.NoName, false, "NoName"));
            KeyBoardKeys.Add(Key.Pa1, new KeySpec((ushort)Key.Pa1, false, "Pa1"));
            KeyBoardKeys.Add(Key.OemClear, new KeySpec((ushort)Key.OemClear, false, "OemClear"));
            KeyBoardKeys.Add(Key.DeadCharProcessed, new KeySpec((ushort)Key.DeadCharProcessed, false, "DeadCharProcessed"));
        }       
    }
}