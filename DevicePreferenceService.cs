using System;
using System.Collections.Generic;
using System.Text;

namespace SoundMeter
{
    internal sealed class ConfiguredDevice
    {
        public ConfiguredDevice(int slot, string label, AudioDevice device)
        {
            Slot = slot;
            Label = label;
            Device = device;
        }

        public int Slot { get; private set; }
        public string Label { get; private set; }
        public AudioDevice Device { get; private set; }
    }

    internal sealed class DevicePreferenceService
    {
        public bool EnsureDefaults(AppSettings settings, IList<AudioDevice> devices)
        {
            if (settings.HasDeviceSelection())
            {
                return false;
            }

            AudioDevice first = FindDevice(devices, IsUsbSpeaker);
            AudioDevice second = FindDevice(devices, IsVoicemeeterInput);

            if (first == null)
            {
                first = FindDefaultDevice(devices);
            }

            if (second == null)
            {
                second = FindFirstOtherDevice(devices, first);
            }

            if (first == null && devices.Count > 0)
            {
                first = devices[0];
            }

            if (second == null && devices.Count > 1)
            {
                second = devices[1];
            }

            ApplyDeviceOne(settings, first, "스피커");
            ApplyDeviceTwo(settings, second, "가상 오디오");
            return first != null || second != null;
        }

        public IList<ConfiguredDevice> GetConfiguredDevices(AppSettings settings, IList<AudioDevice> devices)
        {
            var configured = new List<ConfiguredDevice>();

            AudioDevice first = MatchDevice(devices, settings.DeviceOneId, settings.DeviceOneName);
            AudioDevice second = MatchDevice(devices, settings.DeviceTwoId, settings.DeviceTwoName);

            if (first != null)
            {
                configured.Add(new ConfiguredDevice(1, CleanLabel(settings.DeviceOneAlias, "출력 1"), first));
            }

            if (second != null && !ReferenceEquals(first, second))
            {
                configured.Add(new ConfiguredDevice(2, CleanLabel(settings.DeviceTwoAlias, "출력 2"), second));
            }

            return configured;
        }

        public void ApplyDeviceOne(AppSettings settings, AudioDevice device, string alias)
        {
            if (device == null)
            {
                return;
            }

            settings.DeviceOneId = device.Id;
            settings.DeviceOneName = device.Name;
            settings.DeviceOneAlias = CleanLabel(alias, "Output 1");
        }

        public void ApplyDeviceTwo(AppSettings settings, AudioDevice device, string alias)
        {
            if (device == null)
            {
                return;
            }

            settings.DeviceTwoId = device.Id;
            settings.DeviceTwoName = device.Name;
            settings.DeviceTwoAlias = CleanLabel(alias, "Output 2");
        }

        public AudioDevice MatchDevice(IList<AudioDevice> devices, string deviceId, string deviceName)
        {
            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                foreach (var device in devices)
                {
                    if (string.Equals(device.Id, deviceId, StringComparison.OrdinalIgnoreCase))
                    {
                        return device;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(deviceName))
            {
                foreach (var device in devices)
                {
                    if (string.Equals(device.Name, deviceName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return device;
                    }
                }
            }

            return null;
        }

        private static AudioDevice FindDefaultDevice(IList<AudioDevice> devices)
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

        private static AudioDevice FindFirstOtherDevice(IList<AudioDevice> devices, AudioDevice current)
        {
            foreach (var device in devices)
            {
                if (!ReferenceEquals(device, current))
                {
                    return device;
                }
            }

            return null;
        }

        private static AudioDevice FindDevice(IList<AudioDevice> devices, Predicate<string> predicate)
        {
            foreach (var device in devices)
            {
                if (predicate(device.Name))
                {
                    return device;
                }
            }

            return null;
        }

        private static bool IsUsbSpeaker(string name)
        {
            string normalized = NormalizeName(name);
            return (normalized.Contains("speaker") || normalized.Contains("스피커"))
                && normalized.Contains("usbaudiodevice");
        }

        private static bool IsVoicemeeterInput(string name)
        {
            string normalized = NormalizeName(name);
            return normalized.StartsWith("voicemeeterinput(")
                && normalized.Contains("voicemeetervaio");
        }

        private static string NormalizeName(string value)
        {
            var builder = new StringBuilder();
            foreach (char character in value ?? string.Empty)
            {
                if (!char.IsWhiteSpace(character) && character != '-' && character != '_')
                {
                    builder.Append(char.ToLowerInvariant(character));
                }
            }

            return builder.ToString();
        }

        private static string CleanLabel(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}
