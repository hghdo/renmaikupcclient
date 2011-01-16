using System;
using System.Runtime.InteropServices;

namespace Snarl
{
    class Win32
    {        
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_CLOSE = 0xF060;
        public const int WM_COPYDATA = 0x4A;
        public const int HWND_MESSAGE = 0x84;
        public const int WM_USER = 0x400;
        [DllImport("user32.dll")]
        public static extern int FindWindow(
        string lpClassName, // class name
        string lpWindowName // window name
        );
        [DllImport("user32.dll")]
        public static extern int SendMessage(
        int hWnd, // handle to destination window
        uint Msg, // message
        uint wParam, // first message parameter
        IntPtr lParam // second message parameter
        );  
        [DllImport("user32.dll")]
        public static extern IntPtr CreateWindowEx(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam
        );
        [DllImport("user32.dll")]
        public static extern bool DestroyWindow(
        IntPtr hWnd
        );
   }
}
