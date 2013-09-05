using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Styx;

namespace AdvancedAI.Helpers
{
    class KeyboardPolling
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        private static extern short GetAsyncKeyState(Keys vKey);

        public static bool IsKeyDown(Keys key)
        {
            if (GetActiveWindow() != StyxWoW.Memory.Process.MainWindowHandle)
                return false;
            return (GetAsyncKeyState(key)) != 0;
        }
    }
}
