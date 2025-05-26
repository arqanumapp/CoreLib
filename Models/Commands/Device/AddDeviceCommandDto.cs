using MessagePack;

namespace CoreLib.Models.Commands.Device
{
    [MessagePackObject]
    internal class AddDeviceCommandDto
    {
        [Key(0)] public string DeviceId { get; set; }

        [Key(1)] public string DeviceName { get; set; }

        [Key(2)] public byte[] DeviceSPK { get; set; }

        [Key(3)] public byte[] DevicePK { get; set; }
    }
}
