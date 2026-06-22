using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SoundMeter
{
    internal sealed class QuickSwitchForm : Form
    {
        private readonly IList<ConfiguredDevice> devices;
        private readonly AudioDevice currentDevice;
        private readonly Action<ConfiguredDevice> switchRequested;
        private readonly Action settingsRequested;
        private readonly Action exitRequested;
        private readonly string hotkeyText;
        private readonly bool hotkeyRegistered;

        public QuickSwitchForm(
            IList<ConfiguredDevice> devices,
            AudioDevice currentDevice,
            string hotkeyText,
            bool hotkeyRegistered,
            Action<ConfiguredDevice> switchRequested,
            Action settingsRequested,
            Action exitRequested)
        {
            this.devices = devices;
            this.currentDevice = currentDevice;
            this.hotkeyText = hotkeyText;
            this.hotkeyRegistered = hotkeyRegistered;
            this.switchRequested = switchRequested;
            this.settingsRequested = settingsRequested;
            this.exitRequested = exitRequested;

            Font = new Font("Segoe UI", 9F);
            BackColor = Color.FromArgb(245, 245, 245);
            ForeColor = Color.FromArgb(32, 32, 32);
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            Width = 390;
            Height = 310 + Math.Max(0, devices.Count - 2) * 72;

            BuildLayout();
        }

        public void ShowNearCursor()
        {
            Rectangle workingArea = Screen.FromPoint(Cursor.Position).WorkingArea;
            int x = Cursor.Position.X - Width + 20;
            int y = Cursor.Position.Y - Height - 12;

            if (x < workingArea.Left + 8)
            {
                x = workingArea.Left + 8;
            }

            if (x + Width > workingArea.Right - 8)
            {
                x = workingArea.Right - Width - 8;
            }

            if (y < workingArea.Top + 8)
            {
                y = Cursor.Position.Y + 12;
            }

            if (y + Height > workingArea.Bottom - 8)
            {
                y = workingArea.Bottom - Height - 8;
            }

            Location = new Point(x, y);
            Show();
            Activate();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle = cp.ClassStyle | 0x00020000;
                return cp;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            NativeUi.ApplyRoundedCorners(this);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            Close();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(Color.FromArgb(225, 225, 225)))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private void BuildLayout()
        {
            var root = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 250, 250)
            };
            Controls.Add(root);

            var title = new Label
            {
                Text = "\U0001F50A  Sound Meter",
                AutoSize = false,
                Location = new Point(16, 14),
                Size = new Size(350, 28),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            root.Controls.Add(title);

            var subtitle = new Label
            {
                Text = currentDevice == null ? "현재 출력: 알 수 없음" : "현재 출력: " + currentDevice.Name,
                AutoSize = false,
                Location = new Point(16, 42),
                Size = new Size(350, 24),
                ForeColor = Color.FromArgb(96, 96, 96),
                TextAlign = ContentAlignment.MiddleLeft
            };
            root.Controls.Add(subtitle);

            var footer = BuildFooter();
            footer.Location = new Point(16, Height - 72);
            root.Controls.Add(footer);

            var list = new FlowLayoutPanel
            {
                Location = new Point(16, 76),
                Size = new Size(350, Math.Max(74, Height - 150)),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0),
                BackColor = root.BackColor
            };
            root.Controls.Add(list);

            if (devices.Count == 0)
            {
                var empty = new Label
                {
                    Text = "설정된 출력 장치를 찾지 못했습니다.",
                    Width = 350,
                    Height = 68,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.FromArgb(96, 96, 96)
                };
                list.Controls.Add(empty);
            }
            else
            {
                foreach (var configured in devices)
                {
                    var tile = new DeviceTile(configured, IsCurrent(configured.Device));
                    tile.Width = 350;
                    tile.Height = 66;
                    tile.Margin = new Padding(0, 0, 0, 8);
                    tile.Click += delegate
                    {
                        Close();
                        switchRequested(configured);
                    };
                    list.Controls.Add(tile);
                }
            }
        }

        private Control BuildFooter()
        {
            var footer = new Panel
            {
                Size = new Size(350, 58),
                Height = 58,
                BackColor = Color.FromArgb(250, 250, 250)
            };

            var hotkey = new Label
            {
                Text = hotkeyRegistered ? hotkeyText : hotkeyText + " 사용 불가",
                AutoSize = false,
                Location = new Point(0, 6),
                Size = new Size(170, 34),
                ForeColor = hotkeyRegistered ? Color.FromArgb(90, 90, 90) : Color.FromArgb(160, 70, 70),
                TextAlign = ContentAlignment.MiddleLeft
            };
            footer.Controls.Add(hotkey);

            var settings = CreateFooterButton("설정");
            settings.Location = new Point(180, 9);
            settings.Click += delegate
            {
                Close();
                settingsRequested();
            };
            footer.Controls.Add(settings);

            var exit = CreateFooterButton("종료");
            exit.Location = new Point(272, 9);
            exit.Click += delegate
            {
                Close();
                exitRequested();
            };
            footer.Controls.Add(exit);

            return footer;
        }

        private static Button CreateFooterButton(string text)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(78, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(245, 245, 245),
                ForeColor = Color.FromArgb(32, 32, 32),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 210);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 243, 252);
            return button;
        }

        private bool IsCurrent(AudioDevice device)
        {
            return currentDevice != null
                && device != null
                && string.Equals(currentDevice.Id, device.Id, StringComparison.OrdinalIgnoreCase);
        }
    }

    internal sealed class DeviceTile : Control
    {
        private readonly ConfiguredDevice configuredDevice;
        private readonly bool isCurrent;

        public DeviceTile(ConfiguredDevice configuredDevice, bool isCurrent)
        {
            this.configuredDevice = configuredDevice;
            this.isCurrent = isCurrent;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 9F);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            bool hover = ClientRectangle.Contains(PointToClient(Cursor.Position));
            Color background = isCurrent
                ? Color.FromArgb(230, 244, 255)
                : hover ? Color.FromArgb(243, 248, 253) : Color.White;
            Color border = isCurrent ? Color.FromArgb(0, 120, 212) : Color.FromArgb(224, 224, 224);

            using (GraphicsPath path = CreateRoundRect(bounds, 8))
            using (var brush = new SolidBrush(background))
            using (var pen = new Pen(border))
            {
                graphics.FillPath(brush, path);
                graphics.DrawPath(pen, path);
            }

            if (isCurrent)
            {
                using (var brush = new SolidBrush(Color.FromArgb(0, 120, 212)))
                {
                    graphics.FillRectangle(brush, 0, 12, 4, Height - 24);
                }
            }

            TextRenderer.DrawText(
                graphics,
                configuredDevice.Label,
                new Font("Segoe UI", 10F, FontStyle.Bold),
                new Rectangle(18, 10, Width - 74, 22),
                Color.FromArgb(32, 32, 32),
                TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);

            TextRenderer.DrawText(
                graphics,
                configuredDevice.Device.Name,
                Font,
                new Rectangle(18, 34, Width - 74, 20),
                Color.FromArgb(96, 96, 96),
                TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);

            if (isCurrent)
            {
                TextRenderer.DrawText(
                    graphics,
                    "\u2713",
                    new Font("Segoe UI", 15F, FontStyle.Bold),
                    new Rectangle(Width - 42, 16, 26, 28),
                    Color.FromArgb(0, 120, 212),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private static GraphicsPath CreateRoundRect(Rectangle rect, int radius)
        {
            int diameter = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
