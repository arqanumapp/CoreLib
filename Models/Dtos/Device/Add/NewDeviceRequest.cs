using MessagePack;

namespace CoreLib.Models.Dtos.Device.Add
{
    [MessagePackObject]
    public class NewDeviceRequest
    {
        [Key(0)] public byte[] PrivatePayload { get; set; }
        [Key(1)] public byte[] PublicPayload { get; set; }
        [Key(2)] public string TrustedDeviceId { get; set; }
        [Key(3)] public byte[] TrustedSignature { get; set; }
        [Key(4)] public byte[] PrivatePayloadHash { get; set; }
        [Key(5)] public byte[] PublicPayloadHash { get; set; }
        [Key(6)] public string TempId { get; set; }
        [Key(7)] public long Timestamp { get; set; }
    }
}
