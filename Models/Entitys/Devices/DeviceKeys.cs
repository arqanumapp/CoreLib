using SQLite;

namespace CoreLib.Models.Entitys.Devices
{
    [Table("DeviceKeys")]
    public class DeviceKeys
    {
        [PrimaryKey] public string DeviceId { get; set; }

        [NotNull] public byte[] SPK { get; set; }

        public byte[] SPrK { get; set; }

        [NotNull] public byte[] PK { get; set; }

        public byte[] PrK { get; set; }
    }
}
