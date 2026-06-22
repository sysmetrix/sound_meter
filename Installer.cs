using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SoundMeterInstaller
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new InstallerForm());
        }
    }

    internal sealed class InstallerForm : Form
    {
        private CheckBox startWithWindows;
        private CheckBox desktopShortcut;
        private CheckBox launchAfterInstall;
        private Label status;
        private readonly string installDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs",
            "SoundMeter");

        public InstallerForm()
        {
            Text = "Sound Meter 설치";
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.FromArgb(250, 250, 250);
            ForeColor = Color.FromArgb(32, 32, 32);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(520, 320);

            BuildLayout();
        }

        private void BuildLayout()
        {
            var title = new Label
            {
                Text = "Sound Meter",
                Location = new Point(26, 22),
                Size = new Size(300, 32),
                Font = new Font("Segoe UI", 16F, FontStyle.Bold)
            };
            Controls.Add(title);

            var subtitle = new Label
            {
                Text = "Windows 출력 장치를 빠르게 전환하는 트레이 유틸리티입니다.",
                Location = new Point(28, 58),
                Size = new Size(450, 24),
                ForeColor = Color.FromArgb(96, 96, 96)
            };
            Controls.Add(subtitle);

            var installPath = new Label
            {
                Text = "설치 위치: " + installDirectory,
                Location = new Point(30, 92),
                Size = new Size(460, 38),
                ForeColor = Color.FromArgb(72, 72, 72)
            };
            Controls.Add(installPath);

            startWithWindows = CreateCheckBox("Windows 시작 시 자동 실행", 30, 142, true);
            desktopShortcut = CreateCheckBox("바탕화면 바로가기 만들기", 30, 174, true);
            launchAfterInstall = CreateCheckBox("설치 후 바로 실행", 30, 206, true);

            Controls.Add(startWithWindows);
            Controls.Add(desktopShortcut);
            Controls.Add(launchAfterInstall);

            status = new Label
            {
                Text = string.Empty,
                Location = new Point(30, 264),
                Size = new Size(260, 28),
                ForeColor = Color.FromArgb(96, 96, 96),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(status);

            var cancel = CreateButton("취소", false);
            cancel.Location = new Point(334, 266);
            cancel.Click += delegate { Close(); };
            Controls.Add(cancel);

            var install = CreateButton("설치", true);
            install.Location = new Point(426, 266);
            install.Click += InstallOnClick;
            Controls.Add(install);
        }

        private static CheckBox CreateCheckBox(string text, int x, int y, bool isChecked)
        {
            return new CheckBox
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(250, 24),
                Checked = isChecked,
                FlatStyle = FlatStyle.System
            };
        }

        private static Button CreateButton(string text, bool accent)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(78, 32),
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

        private void InstallOnClick(object sender, EventArgs e)
        {
            try
            {
                status.Text = "설치 중...";
                UseWaitCursor = true;
                Install();
                status.Text = "설치 완료";
                MessageBox.Show("Sound Meter 설치가 완료되었습니다.", "Sound Meter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                status.Text = "설치 실패";
                MessageBox.Show(ex.Message, "Sound Meter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private void Install()
        {
            string installDir = installDirectory;
            string exePath = Path.Combine(installDir, "SoundMeter.exe");

            Directory.CreateDirectory(installDir);
            StopInstalledInstance(exePath);
            File.WriteAllBytes(exePath, Convert.FromBase64String(InstallerPayload.SoundMeterExeBase64));
            WriteUninstaller(installDir);

            string startMenuDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Microsoft",
                "Windows",
                "Start Menu",
                "Programs",
                "Sound Meter");
            Directory.CreateDirectory(startMenuDir);
            CreateShortcut(Path.Combine(startMenuDir, "Sound Meter.lnk"), exePath, string.Empty, exePath);
            CreateShortcut(Path.Combine(startMenuDir, "Sound Meter 제거.lnk"), Path.Combine(installDir, "UninstallSoundMeter.cmd"), string.Empty, exePath);

            if (desktopShortcut.Checked)
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                CreateShortcut(Path.Combine(desktop, "Sound Meter.lnk"), exePath, string.Empty, exePath);
            }

            SetStartup(exePath, startWithWindows.Checked);

            if (launchAfterInstall.Checked)
            {
                Process.Start(exePath);
            }
        }

        private static void StopInstalledInstance(string exePath)
        {
            foreach (Process process in Process.GetProcessesByName("SoundMeter"))
            {
                try
                {
                    string runningPath = process.MainModule.FileName;
                    if (string.Equals(runningPath, exePath, StringComparison.OrdinalIgnoreCase))
                    {
                        process.Kill();
                        process.WaitForExit(3000);
                    }
                }
                catch
                {
                }
            }
        }

        private static void WriteUninstaller(string installDir)
        {
            string path = Path.Combine(installDir, "UninstallSoundMeter.cmd");
            string content =
                "@echo off\r\n" +
                "taskkill /IM SoundMeter.exe /F >nul 2>nul\r\n" +
                "reg delete HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run /v SoundMeter /f >nul 2>nul\r\n" +
                "del \"%USERPROFILE%\\Desktop\\Sound Meter.lnk\" >nul 2>nul\r\n" +
                "rmdir /S /Q \"%APPDATA%\\Microsoft\\Windows\\Start Menu\\Programs\\Sound Meter\" >nul 2>nul\r\n" +
                "cd /d \"%LOCALAPPDATA%\\Programs\"\r\n" +
                "rmdir /S /Q \"SoundMeter\"\r\n";
            File.WriteAllText(path, content);
        }

        private static void SetStartup(string exePath, bool enabled)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                "Software\\Microsoft\\Windows\\CurrentVersion\\Run",
                true))
            {
                if (key == null)
                {
                    return;
                }

                if (enabled)
                {
                    key.SetValue("SoundMeter", "\"" + exePath + "\"");
                }
                else
                {
                    key.DeleteValue("SoundMeter", false);
                }
            }
        }

        private static void CreateShortcut(string shortcutPath, string targetPath, string arguments, string iconPath)
        {
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            object shell = Activator.CreateInstance(shellType);
            object shortcut = shellType.InvokeMember(
                "CreateShortcut",
                BindingFlags.InvokeMethod,
                null,
                shell,
                new object[] { shortcutPath });

            Type shortcutType = shortcut.GetType();
            shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { targetPath });
            shortcutType.InvokeMember("Arguments", BindingFlags.SetProperty, null, shortcut, new object[] { arguments });
            shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { Path.GetDirectoryName(targetPath) });
            shortcutType.InvokeMember("IconLocation", BindingFlags.SetProperty, null, shortcut, new object[] { iconPath });
            shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
        }
    }
}
