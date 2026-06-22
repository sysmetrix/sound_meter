using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SoundMeter
{
    internal sealed class SoundTrayApp : ApplicationContext
    {
        private readonly NotifyIcon notifyIcon;
        private readonly Icon trayIcon;
        private readonly AudioDeviceService audioService = new AudioDeviceService();
        private readonly DevicePreferenceService preferenceService = new DevicePreferenceService();
        private readonly HotkeyService hotkeyService;

        private AppSettings settings;
        private bool hotkeyRegistered;
        private QuickSwitchForm popup;

        public SoundTrayApp()
        {
            settings = AppSettings.Load();
            trayIcon = AudioIconFactory.CreateTrayIcon();

            notifyIcon = new NotifyIcon
            {
                Icon = trayIcon,
                Text = "Sound Meter",
                Visible = true
            };

            notifyIcon.MouseUp += NotifyIconOnMouseUp;
            hotkeyService = new HotkeyService(TogglePreferredOutput);
            RegisterHotkey();
            InitializeDefaultSettings();
            UpdateTrayText();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (popup != null)
                {
                    popup.Close();
                    popup.Dispose();
                    popup = null;
                }

                hotkeyService.Dispose();
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                trayIcon.Dispose();
            }

            base.Dispose(disposing);
        }

        private void NotifyIconOnMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowQuickSwitcher();
            }
            else if (e.Button == MouseButtons.Right)
            {
                ShowTrayMenu();
            }
        }

        private void ShowQuickSwitcher()
        {
            try
            {
                IList<AudioDevice> allDevices = audioService.GetOutputDevices();
                if (preferenceService.EnsureDefaults(settings, allDevices))
                {
                    settings.Save();
                }

                IList<ConfiguredDevice> configured = preferenceService.GetConfiguredDevices(settings, allDevices);
                AudioDevice current = FindCurrentDevice(allDevices);

                if (popup != null)
                {
                    popup.Close();
                    popup.Dispose();
                    popup = null;
                }

                popup = new QuickSwitchForm(
                    configured,
                    current,
                    settings.GetHotkeyText(),
                    hotkeyRegistered,
                    delegate(ConfiguredDevice device) { SwitchToDevice(device.Device, "Output changed"); },
                    OpenSettings,
                    ExitThread);

                popup.FormClosed += delegate { popup = null; };
                popup.ShowNearCursor();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void ShowTrayMenu()
        {
            var menu = new ContextMenuStrip();

            try
            {
                IList<AudioDevice> devices = audioService.GetOutputDevices();
                AudioDevice current = FindCurrentDevice(devices);
                menu.Items.Add(current == null ? "Current output: unknown" : "Current output: " + current.Name).Enabled = false;
            }
            catch
            {
                menu.Items.Add("Current output: unavailable").Enabled = false;
            }

            menu.Items.Add(hotkeyRegistered ? "Hotkey: " + settings.GetHotkeyText() : "Hotkey unavailable: " + settings.GetHotkeyText()).Enabled = false;
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Open", null, delegate { ShowQuickSwitcher(); });
            menu.Items.Add("Settings", null, delegate { OpenSettings(); });
            menu.Items.Add("Refresh", null, delegate { UpdateTrayText(); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit", null, delegate { ExitThread(); });

            menu.Closed += delegate { menu.Dispose(); };
            menu.Show(Cursor.Position);
        }

        private void OpenSettings()
        {
            try
            {
                IList<AudioDevice> devices = audioService.GetOutputDevices();
                if (devices.Count == 0)
                {
                    MessageBox.Show("No active output devices were found.", "Sound Meter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var form = new SettingsForm(settings, devices, preferenceService))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        settings = AppSettings.Load();
                        RegisterHotkey();
                        UpdateTrayText();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void TogglePreferredOutput()
        {
            try
            {
                IList<AudioDevice> allDevices = audioService.GetOutputDevices();
                if (preferenceService.EnsureDefaults(settings, allDevices))
                {
                    settings.Save();
                }

                IList<ConfiguredDevice> configured = preferenceService.GetConfiguredDevices(settings, allDevices);
                if (configured.Count < 2)
                {
                    notifyIcon.ShowBalloonTip(2500, "Switch unavailable", "Open Settings and choose two connected outputs.", ToolTipIcon.Warning);
                    OpenSettings();
                    return;
                }

                int currentIndex = -1;
                for (int index = 0; index < configured.Count; index++)
                {
                    if (configured[index].Device.IsDefault)
                    {
                        currentIndex = index;
                        break;
                    }
                }

                ConfiguredDevice next = currentIndex == 0 ? configured[1] : configured[0];
                SwitchToDevice(next.Device, "Hotkey switch");
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void SwitchToDevice(AudioDevice device, string title)
        {
            try
            {
                audioService.SetDefaultOutputDevice(device.Id);
                notifyIcon.ShowBalloonTip(1400, title, device.Name, ToolTipIcon.Info);
                UpdateTrayText();
            }
            catch (Exception ex)
            {
                notifyIcon.ShowBalloonTip(2500, "Switch failed", "Check that the device is connected.", ToolTipIcon.Warning);
                ShowError(ex);
            }
        }

        private void RegisterHotkey()
        {
            hotkeyRegistered = hotkeyService.Register(settings.GetHotkeyKey());
        }

        private void InitializeDefaultSettings()
        {
            try
            {
                IList<AudioDevice> devices = audioService.GetOutputDevices();
                if (preferenceService.EnsureDefaults(settings, devices))
                {
                    settings.Save();
                }
            }
            catch
            {
                // The tray menu will expose the detailed error when the user opens it.
            }
        }

        private void UpdateTrayText()
        {
            try
            {
                AudioDevice current = FindCurrentDevice(audioService.GetOutputDevices());
                notifyIcon.Text = current == null ? "Sound Meter" : TrimForTray("Sound Meter: " + current.Name);
            }
            catch
            {
                notifyIcon.Text = "Sound Meter";
            }
        }

        private static AudioDevice FindCurrentDevice(IList<AudioDevice> devices)
        {
            foreach (var device in devices)
            {
                if (device.IsDefault)
                {
                    return device;
                }
            }

            return null;
        }

        private static string TrimForTray(string value)
        {
            return value.Length <= 63 ? value : value.Substring(0, 60) + "...";
        }

        private static void ShowError(Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Sound Meter",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
