using MessagePack;

namespace CoreLib.Models.Dtos.Contact.Lookup
{
    [MessagePackObject]
    internal class LookupUserRequest
    {
        [Key(0)] public string DeviceId { get; set; }
        [Key(1)] public string AccountId { get; set; }
    }
}
