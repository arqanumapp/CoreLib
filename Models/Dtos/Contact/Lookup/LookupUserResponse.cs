using MessagePack;

namespace CoreLib.Models.Dtos.Contact.Lookup
{
    [MessagePackObject]
    internal class LookupUserResponse
    {
        [Key(0)] public string Nick { get; set; }
        [Key(1)] public long Timetamp { get; set; }
    }
}
