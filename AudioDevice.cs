namespace SoundMeter
{
    internal sealed class AudioDevice
    {
        public AudioDevice(string id, string name, bool isDefault)
        {
            Id = id;
            Name = name;
            IsDefault = isDefault;
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public bool IsDefault { get; private set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
