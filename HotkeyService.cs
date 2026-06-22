using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SoundMeter
{
    internal sealed class HotkeyService : NativeWindow, IDisposable
    {
        private const int HOTKEY_ID = 0x534D01;
        private const int WM_HOTKEY = 0x0312;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;

        private readonly Action onHotkeyPressed;
        private bool registered;

        public HotkeyService(Action onHotkeyPressed)
        {
            this.onHotkeyPressed = onHotkeyPressed;
            CreateHandle(new CreateParams());
        }

        public bool Register(Keys key)
        {
            if (registered)
            {
                UnregisterHotKey(Handle, HOTKEY_ID);
                registered = false;
            }

            registered = RegisterHotKey(Handle, HOTKEY_ID, MOD_CONTROL | MOD_ALT, (uint)key);
            return registered;
        }

        public void Dispose()
        {
            if (registered)
            {
                UnregisterHotKey(Handle, HOTKEY_ID);
                registered = false;
            }

            DestroyHandle();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                onHotkeyPressed();
                return;
            }

            base.WndProc(ref m);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
