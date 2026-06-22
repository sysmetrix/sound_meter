using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SoundMeter
{
    internal static class NativeUi
    {
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;

        public static void ApplyRoundedCorners(Form form)
        {
            if (Environment.OSVersion.Version.Major < 10)
            {
                return;
            }

            int preference = DWMWCP_ROUND;
            DwmSetWindowAttribute(
                form.Handle,
                DWMWA_WINDOW_CORNER_PREFERENCE,
                ref preference,
                sizeof(int));
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, int cbAttribute);
    }
}
