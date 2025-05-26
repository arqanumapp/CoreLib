using MessagePack;

namespace CoreLib.Models.Commands
{
    [MessagePackObject]
    internal class CommandEnvelopeContent
    {
        [Key(0)]
        public CommandType CommandType { get; set; }

        [Key(1)]
        public string SenderId { get; set; }

        [Key(2)]
        public string RecipientId { get; set; }

        [Key(3)]
        public byte[] EncryptedPayload { get; set; }

        [Key(4)]
        public long Timestamp { get; set; }
    }
}
