using MessagePack;
using System.Collections.Immutable;

namespace CoreLib.Models.Dtos.Account.Create
{
    [MessagePackObject]
    internal class RegisterDeviceRequest
    {
        [Key(0)] public string Id { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public byte[] SPK { get; set; } 
        [Key(3)] public byte[] Signature { get; set; } 
        [Key(4)] public ImmutableArray<RegisterPreKeyRequest> PreKeys { get; set; }
    }
}
