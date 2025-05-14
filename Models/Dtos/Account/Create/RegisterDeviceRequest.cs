namespace CoreLib.Models.Dtos.Account.Create
{
    internal class RegisterDeviceRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SPK { get; set; } //Base 64
        public string Signature { get; set; } //Base 64
        public List<RegisterPreKeyRequest> PreKeys { get; set; }
    }
}
