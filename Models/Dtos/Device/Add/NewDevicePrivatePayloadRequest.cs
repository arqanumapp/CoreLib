using MessagePack;
using System.Collections.Immutable;

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
        [Key(5)] public byte[] PK { get; set; }
        [Key(6)] public byte[] PrK { get; set; }
        [Key(7)] public ImmutableArray<TrustedDevice> TrustedDevices { get; set; }
    }

    [MessagePackObject]
    internal class TrustedDevice
    {
        [Key(0)] public string DeviceId { get; set; }

        [Key(1)] public string Name { get; set; }

        [Key(2)] public byte[] SPK { get; set; }

        [Key(3)] public byte[] PK { get; set; }
    }
}
