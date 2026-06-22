using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace SoundMeter
{
    internal static class AudioIconFactory
    {
        public static Icon CreateTrayIcon()
        {
            using (var bitmap = new Bitmap(64, 64))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                using (var brush = new SolidBrush(Color.FromArgb(0, 120, 212)))
                {
                    graphics.FillEllipse(brush, 3, 3, 58, 58);
                }

                using (var font = new Font("Segoe UI Emoji", 30, FontStyle.Regular, GraphicsUnit.Pixel))
                using (var brush = new SolidBrush(Color.White))
                using (var format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                    graphics.DrawString("\U0001F50A", font, brush, new RectangleF(0, 1, 64, 62), format);
                }

                IntPtr handle = bitmap.GetHicon();
                try
                {
                    return (Icon)Icon.FromHandle(handle).Clone();
                }
                finally
                {
                    DestroyIcon(handle);
                }
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);
    }
}
