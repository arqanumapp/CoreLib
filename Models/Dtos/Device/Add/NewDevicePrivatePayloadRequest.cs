using MessagePack;

namespace CoreLib.Models.Dtos.Device.Add
{
    [MessagePackObject]
    internal class NewDevicePrivatePayloadRequest
    {
        [Key(0)] public string AccountId { get; set; }
        [Key(1)] public string DeviceId { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public byte[] SPK { get; set; }
        [Key(4)] public byte[] SPrK { get; set; }
        [Key(5)] public byte[] SPKSignature { get; set; }
    }
}
