using MessagePack;

namespace CoreLib.Models.Dtos.Device.Add
{
    [MessagePackObject]
    public class NewDeviceRequest
    {
        [Key(0)] public byte[] Payload { get; set; }
        [Key(1)] public string TrustedDeviceId { get; set; }
        [Key(2)] public byte[] TrustedSignature { get; set; }
        [Key(3)] public byte[] PayloadHash { get; set; }
        [Key(4)] public string TempId { get; set; }
        [Key(5)] public long Timestamp { get; set; }
    }
}
