using SQLite;

namespace CoreLib.Models.Entitys
{
    [Table("PreKeys")]
    public class PreKey
    {
        /// <summary>
        /// Shake256 (PK)
        /// </summary>
        [PrimaryKey] public string Id { get; set; }

        public string DeviceId { get; set; }

        [NotNull] public byte[] PK { get; set; }

        [NotNull] public byte[] PrK { get; set; }

        /// <summary>
        /// Signature of the PK
        /// </summary>
        [NotNull] public byte[] Signature { get; set; }
    }
}
