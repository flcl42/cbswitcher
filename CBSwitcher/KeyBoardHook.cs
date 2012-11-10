using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CBSwitcher
{
    public class KeyBoardHook
    {
        #region fields
        public static int MOD_ALT = 0x1;
        public static int MOD_CONTROL = 0x2;
        public static int MOD_SHIFT = 0x4;
        public static int MOD_WIN = 0x8;
        public static int WM_HOTKEY = 0x312;
        #endregion

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private static int keyId;
        private static IntPtr handle;
        public KeyBoardHook(IntPtr handle)
        {
            KeyBoardHook.handle = handle;

            int key = 9;
            keyId = handle.ToInt32();
            RegisterHotKey(handle, keyId, MOD_SHIFT, key);
        }

        public delegate void HotKey();
        public HotKey OnHotKey;

        ~KeyBoardHook()
        {
            UnregisterHotKey(handle, keyId);
        }
    }
   
}
