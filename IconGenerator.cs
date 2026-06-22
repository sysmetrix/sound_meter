using System;
using System.Drawing;
using System.IO;

namespace SoundMeter
{
    internal static class IconGenerator
    {
        private static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Usage: IconGenerator <output.ico>");
                return 1;
            }

            string outputPath = args[0];
            string directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (Icon icon = AudioIconFactory.CreateTrayIcon())
            using (FileStream stream = File.Create(outputPath))
            {
                icon.Save(stream);
            }

            return 0;
        }
    }
}
