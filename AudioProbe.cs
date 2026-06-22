using System;

namespace SoundMeter
{
    internal static class AudioProbe
    {
        private static int Main()
        {
            try
            {
                var service = new AudioDeviceService();
                var devices = service.GetOutputDevices();

                Console.WriteLine("Active output devices: " + devices.Count);
                foreach (var device in devices)
                {
                    Console.WriteLine((device.IsDefault ? "* " : "  ") + device.Name);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
        }
    }
}
