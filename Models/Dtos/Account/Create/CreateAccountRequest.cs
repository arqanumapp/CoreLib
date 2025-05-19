using MessagePack;

namespace CoreLib.Models.Dtos.Account.Create
{
    [MessagePackObject]
    internal class CreateAccountRequest
    {
        [Key(0)] public string Id { get; set; }
        [Key(1)] public string Username { get; set; }
        [Key(2)] public RegisterDeviceRequest Device { get; set; }
        [Key(3)] public string ProofOfWork { get; set; }
        [Key(4)] public string Nonce { get; set; }
        [Key(5)] public string ChaptchaToken { get; set; }
        [Key(6)] public long Timestamp { get; set; }
    }
}
