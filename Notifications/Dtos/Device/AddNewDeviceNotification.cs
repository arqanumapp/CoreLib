using MessagePack;

namespace CoreLib.Notifications.Dtos.Device
{
    [MessagePackObject]
    public class AddNewDeviceNotification
    {
        [Key(0)]public string DeviceId { get; set; }

        [Key(1)] public string DeviceName { get; set; }

        [Key(2)] public byte[] SPK { get; set; }
    }
}
