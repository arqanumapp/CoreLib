using SQLite;

namespace CoreLib.Models.Entitys
{
    [Table("Account")]
    public class Account
    {
        /// <summary>
        /// Shake256 first SPK
        /// </summary>
        [NotNull] public string Id { get; set; }

        [NotNull] public string NickName { get; set; }
    }
}
