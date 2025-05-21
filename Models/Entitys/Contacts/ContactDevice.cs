using SQLite;

namespace CoreLib.Models.Entitys.Contacts
{
    [Table("ContactDevices")]
    internal class ContactDevice
    {
        [NotNull] public string ContactId { get; set; }

        [NotNull] public string DeviceId { get; set; }

        [NotNull] public byte[] PK { get; set; }

        [NotNull] public byte[] SPK { get; set; }
    }
}
