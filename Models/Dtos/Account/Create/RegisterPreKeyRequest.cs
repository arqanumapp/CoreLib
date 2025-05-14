namespace CoreLib.Models.Dtos.Account.Create
{
    internal class RegisterPreKeyRequest
    {
        public string Id { get; set; }
        public string PK { get; set; }
        public string PKSignature { get; set; }
    }
}
