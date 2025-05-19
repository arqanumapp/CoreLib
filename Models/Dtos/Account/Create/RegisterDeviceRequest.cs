using MessagePack;

namespace CoreLib.Models.Dtos.Account.Create
{
    [MessagePackObject]
    internal class RegisterDeviceRequest
    {
        [Key(0)] public string Id { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public byte[] SPK { get; set; } //Base 64
        [Key(3)] public byte[] Signature { get; set; } //Base 64
        [Key(4)] public List<RegisterPreKeyRequest> PreKeys { get; set; }
    }
}
