using MessagePack;

namespace CoreLib.Models.Dtos.Device.Add
{
    [MessagePackObject]
    internal class NewDeviceResponce
    {
        [Key(0)] public byte[] PrivatePayload { get; set; }
        [Key(1)] public byte[] PublicPayload { get; set; }
        [Key(2)] public byte[] TrustedSignature { get; set; }
        [Key(3)] public byte[] PublicPayloadHash { get; set; }
        [Key(4)] public byte[] PrivatePayloadHash { get; set; }
    }
}
