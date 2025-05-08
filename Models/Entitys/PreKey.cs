using SQLite;

namespace CoreLib.Models.Entitys
{
    internal class PreKey
    {
        [PrimaryKey]
        public string Id { get; set; }

        [NotNull]
        public byte[] PK { get; set; }

        [NotNull]
        public byte[] PrK { get; set; }

        [NotNull]
        public byte[] Signature { get; set; }
    }
}
