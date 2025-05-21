using SQLite;

namespace CoreLib.Models.Entitys.Contacts
{
    [Table("Contacts")]
    internal class Contact
    {
        [NotNull] public string AccountId { get; set; }
        [NotNull] public string Nick { get; set; }

        public List<ContactDevice> Devices { get; set; } = new List<ContactDevice>();
    }
}
