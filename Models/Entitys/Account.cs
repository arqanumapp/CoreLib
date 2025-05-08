using SQLite;

namespace CoreLib.Models.Entitys
{
    internal class Account
    {
        /// <summary>
        /// Shake256 first SPK
        /// </summary>
        [NotNull]
        public string Id { get; set; }

        [NotNull]
        public string NickName { get; set; }
    }
}
