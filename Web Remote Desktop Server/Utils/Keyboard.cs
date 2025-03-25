namespace WebRemoteDesktopServer.Utils
{
    static class Keyboard
    {

        public static bool IsExtendedKey(Keys key)
        {
            switch (key)
            {
                case Keys.ShiftKey: case Keys.ShiftLeft: case Keys.ShiftRight: case Keys.Alt: return true;
            }
            return false;
        }
    }
}
