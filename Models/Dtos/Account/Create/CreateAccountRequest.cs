namespace CoreLib.Models.Dtos.Account.Create
{
    internal class CreateAccountRequest
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public RegisterDeviceRequest Device { get; set; }
        public string ProofOfWork { get; set; }
        public string Nonce { get; set; }
        public string ChaptchaToken { get; set; }
        public long Timestamp { get; set; }
    }
}
