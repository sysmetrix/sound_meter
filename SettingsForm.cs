using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SoundMeter
{
    internal sealed class SettingsForm : Form
    {
        private readonly AppSettings settings;
        private readonly IList<AudioDevice> devices;
        private readonly DevicePreferenceService preferenceService;

        private ComboBox deviceOneCombo;
        private ComboBox deviceTwoCombo;
        private TextBox aliasOneText;
        private TextBox aliasTwoText;
        private ComboBox hotkeyCombo;

        public SettingsForm(AppSettings settings, IList<AudioDevice> devices, DevicePreferenceService preferenceService)
        {
            this.settings = settings;
            this.devices = devices;
            this.preferenceService = preferenceService;

            Text = "Sound Meter Settings";
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.FromArgb(250, 250, 250);
            ForeColor = Color.FromArgb(32, 32, 32);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(520, 330);

            BuildLayout();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            NativeUi.ApplyRoundedCorners(this);
        }

        private void BuildLayout()
        {
            var title = new Label
            {
                Text = "Sound Meter",
                Location = new Point(24, 20),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold)
            };
            Controls.Add(title);

            var subtitle = new Label
            {
                Text = "Choose the two outputs and the toggle key.",
                Location = new Point(26, 52),
                Size = new Size(440, 24),
                ForeColor = Color.FromArgb(96, 96, 96)
            };
            Controls.Add(subtitle);

            AddLabel("Output 1", 28, 96);
            deviceOneCombo = CreateDeviceCombo(124, 92);
            Controls.Add(deviceOneCombo);

            AddLabel("Name", 28, 132);
            aliasOneText = CreateTextBox(124, 128, settings.DeviceOneAlias);
            Controls.Add(aliasOneText);

            AddLabel("Output 2", 28, 174);
            deviceTwoCombo = CreateDeviceCombo(124, 170);
            Controls.Add(deviceTwoCombo);

            AddLabel("Name", 28, 210);
            aliasTwoText = CreateTextBox(124, 206, settings.DeviceTwoAlias);
            Controls.Add(aliasTwoText);

            AddLabel("Hotkey", 28, 252);
            hotkeyCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(124, 248),
                Size = new Size(160, 26)
            };
            hotkeyCombo.Items.AddRange(new object[] { "A", "S", "D", "F8", "F9", "F10", "F11", "F12" });
            hotkeyCombo.SelectedItem = string.IsNullOrWhiteSpace(settings.HotkeyKey) ? "A" : settings.HotkeyKey.ToUpperInvariant();
            if (hotkeyCombo.SelectedIndex < 0)
            {
                hotkeyCombo.SelectedItem = "A";
            }
            Controls.Add(hotkeyCombo);

            var cancel = CreateButton("Cancel", false);
            cancel.Location = new Point(316, 284);
            cancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(cancel);

            var save = CreateButton("Save", true);
            save.Location = new Point(414, 284);
            save.Click += SaveOnClick;
            Controls.Add(save);

            PopulateDeviceCombos();
        }

        private void AddLabel(string text, int x, int y)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(86, 24),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(72, 72, 72)
            };
            Controls.Add(label);
        }

        private ComboBox CreateDeviceCombo(int x, int y)
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(x, y),
                Size = new Size(360, 26)
            };
        }

        private TextBox CreateTextBox(int x, int y, string text)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(220, 24),
                Text = text ?? string.Empty
            };
        }

        private Button CreateButton(string text, bool accent)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(82, 32),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            if (accent)
            {
                button.BackColor = Color.FromArgb(0, 120, 212);
                button.ForeColor = Color.White;
                button.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 212);
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 95, 184);
            }
            else
            {
                button.BackColor = Color.FromArgb(245, 245, 245);
                button.ForeColor = Color.FromArgb(32, 32, 32);
                button.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 210);
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 243, 252);
            }

            return button;
        }

        private void PopulateDeviceCombos()
        {
            foreach (var device in devices)
            {
                var item = new DeviceComboItem(device);
                deviceOneCombo.Items.Add(item);
                deviceTwoCombo.Items.Add(item);
            }

            SelectDevice(deviceOneCombo, settings.DeviceOneId, settings.DeviceOneName);
            SelectDevice(deviceTwoCombo, settings.DeviceTwoId, settings.DeviceTwoName);

            if (deviceOneCombo.SelectedIndex < 0 && deviceOneCombo.Items.Count > 0)
            {
                deviceOneCombo.SelectedIndex = 0;
            }

            if (deviceTwoCombo.SelectedIndex < 0 && deviceTwoCombo.Items.Count > 1)
            {
                deviceTwoCombo.SelectedIndex = 1;
            }
        }

        private void SelectDevice(ComboBox combo, string deviceId, string deviceName)
        {
            for (int index = 0; index < combo.Items.Count; index++)
            {
                var item = combo.Items[index] as DeviceComboItem;
                if (item == null)
                {
                    continue;
                }

                AudioDevice match = preferenceService.MatchDevice(
                    new List<AudioDevice> { item.Device },
                    deviceId,
                    deviceName);

                if (match != null)
                {
                    combo.SelectedIndex = index;
                    return;
                }
            }
        }

        private void SaveOnClick(object sender, EventArgs e)
        {
            var first = deviceOneCombo.SelectedItem as DeviceComboItem;
            var second = deviceTwoCombo.SelectedItem as DeviceComboItem;

            if (first == null || second == null)
            {
                MessageBox.Show("Select two output devices.", "Sound Meter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.Equals(first.Device.Id, second.Device.Id, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Choose two different devices.", "Sound Meter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            preferenceService.ApplyDeviceOne(settings, first.Device, aliasOneText.Text);
            preferenceService.ApplyDeviceTwo(settings, second.Device, aliasTwoText.Text);
            settings.HotkeyKey = Convert.ToString(hotkeyCombo.SelectedItem);
            settings.Save();

            DialogResult = DialogResult.OK;
            Close();
        }

        private sealed class DeviceComboItem
        {
            public DeviceComboItem(AudioDevice device)
            {
                Device = device;
            }

            public AudioDevice Device { get; private set; }

            public override string ToString()
            {
                return Device.Name;
            }
        }
    }
}
