using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AdvancedAI.Helpers
{
    class KeyboardPolling
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        public static bool IsKeyDown(Keys key)
        {
            return (GetAsyncKeyState(key)) != 0;
        }
    }
}
