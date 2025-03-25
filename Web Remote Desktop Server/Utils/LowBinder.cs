using System.Runtime.InteropServices;

namespace WebRemoteDesktopServer.Utils
{

    public class LowBinder
    {
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern void keybd_event(int vk, uint scan, int flags, uint extraInfo);

        [DllImport("user32.dll")]
        public static extern void SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorInfo(out CursorInfo pci);

        [DllImport("user32.dll")]
        public static extern int LoadCursor(nint hInstance, int hCursor);

        [DllImport("user32.dll")]
        public static extern int SetCursor(int hCursor);

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(nint hdc, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool AddClipboardFormatListener(nint hWnd);
        [DllImport("user32.dll")]
        public static extern bool RemoveClipboardFormatListener(nint hWnd);
        [DllImport("user32.dll")]
        public static extern nint SetClipboardViewer(nint hWnd);

        [DllImport("user32.dll")]
        public static extern bool ChangeClipboardChain(nint hWndRemove, nint hWndNew);

        [DllImport("user32.dll")]
        public static extern int SendMessage(nint hWnd, int wMsg, nint wParam, nint lParam);

        public enum DeviceCaps
        {
            DesktopVertres = 117,
            DesktopHorzres = 118,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CursorInfo
        {
            public int cbSize;
            public int flags;
            public nint hCursor;
            public POINT ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        public enum CursorType
        {
            None = 0,
            Arrow = 65539,
            IBeam = 65541,
            Wait = 65543,
            Cross = 65545,
            Up = 65547,
            ResizeNWSE = 65549,
            ResizeNESW = 65551,
            ResizeEW = 65553,
            ResizeNS = 65555,
            Move = 65557,
            No = 65559,
            Progress = 65561,
            Pointer = 65563,

            Grabbing = 13896596,
            Alias = 31327887,
            ColResize = 32770565,
            VerticalText = 38668561,
            ZoomIn = 62917193,
            Cell = 64882867,
            Grab = 69339379,
            RowResize = 85000401,
            Copy = 132646983,
            ZoomOut = 186320971
        }

        public enum MouseType
        {
            LeftButtonDown = 0x02,
            LeftButtonUp = 0x04,

            RightButtonDown = 0x08,
            RightButtonUp = 0x10,

            MiddleDown = 0x0020,
            MiddleUp = 0x01,

            XButtonDown = 0x80,
            XButtonUp = 0x100,

            Wheel = 0x0800,
        }

        public enum KeyType
        {
            KeyDown = 0,
            ExtendedKey = 0x01,
            KeyUp = 0x02
        }

        public static CursorInfo GetCursorInfo(out bool result)
        {
            CursorInfo pci;
            pci.cbSize = Marshal.SizeOf(typeof(CursorInfo));
            result = GetCursorInfo(out pci);
            return pci;
        }
    }
}