using SQLite;

namespace CoreLib.Models.Entitys
{
    [Table("Devices")]
    public class Device
    {
        /// <summary>
        /// Shake256 (SPK)
        /// </summary>
        [PrimaryKey]
        public string Id { get; set; }

        [NotNull]
        public string AccountId { get; set; }

        [NotNull]
        public string DeviceName { get; set; }

        [NotNull]
        public byte[] SPK { get; set; }

        [NotNull]
        public byte[] SPrK { get; set; }

        /// <summary>
        /// Signature of the SPK
        /// </summary>
        [NotNull]
        public byte[] SPKSignature { get; set; } 

        public bool CurrentDevice { get; set; }
    }
}
