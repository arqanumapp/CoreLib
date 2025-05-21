using SQLite;

namespace CoreLib.Models.Entitys.Devices
{
    [Table("Devices")]
    public class Device
    {
        /// <summary>
        /// Shake256 (SPK)
        /// </summary>
        [PrimaryKey] public string Id { get; set; }

        [NotNull] public string AccountId { get; set; }

        [NotNull] public string DeviceName { get; set; }

        [NotNull] public string DeviceKeysId { get; set; }

        [Ignore] public DeviceKeys DeviceKeys { get; set; }

        public bool CurrentDevice { get; set; }
    }
}
