using MessagePack;

namespace CoreLib.Models.Dtos.Device.Add
{
    [MessagePackObject]
    internal class NewDevicePreKey
    {
        [Key(0)] public string Id { get; set; }
        [Key(1)] public byte[] SPK { get; set; }
        [Key(2)] public byte[] SPrK { get; set; }
        [Key(3)] public byte[] Signature { get; set; }
    }
}
