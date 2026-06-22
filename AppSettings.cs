using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace SoundMeter
{
    [Serializable]
    public sealed class AppSettings
    {
        public string DeviceOneId { get; set; }
        public string DeviceOneName { get; set; }
        public string DeviceOneAlias { get; set; }
        public string DeviceTwoId { get; set; }
        public string DeviceTwoName { get; set; }
        public string DeviceTwoAlias { get; set; }
        public string HotkeyKey { get; set; }

        public AppSettings()
        {
            DeviceOneAlias = "스피커";
            DeviceTwoAlias = "가상 오디오";
            HotkeyKey = "A";
        }

        public static string SettingsDirectory
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "SoundMeter");
            }
        }

        public static string SettingsPath
        {
            get { return Path.Combine(SettingsDirectory, "settings.xml"); }
        }

        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    return new AppSettings();
                }

                using (var stream = File.OpenRead(SettingsPath))
                {
                    var serializer = new XmlSerializer(typeof(AppSettings));
                    var settings = serializer.Deserialize(stream) as AppSettings;
                    return settings ?? new AppSettings();
                }
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void Save()
        {
            Directory.CreateDirectory(SettingsDirectory);
            using (var stream = File.Create(SettingsPath))
            {
                var serializer = new XmlSerializer(typeof(AppSettings));
                serializer.Serialize(stream, this);
            }
        }

        public Keys GetHotkeyKey()
        {
            Keys key;
            if (!Enum.TryParse(HotkeyKey, true, out key))
            {
                key = Keys.A;
            }

            return key;
        }

        public string GetHotkeyText()
        {
            return "Ctrl + Alt + " + GetHotkeyKey();
        }

        public bool HasDeviceSelection()
        {
            return !string.IsNullOrWhiteSpace(DeviceOneId)
                || !string.IsNullOrWhiteSpace(DeviceOneName)
                || !string.IsNullOrWhiteSpace(DeviceTwoId)
                || !string.IsNullOrWhiteSpace(DeviceTwoName);
        }
    }
}
