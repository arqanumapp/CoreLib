using MessagePack;

namespace CoreLib.Models.Commands
{
    [MessagePackObject]
    internal class CommandEnvelope
    {
        [Key(0)] public byte[] Signature { get; set; }

        [Key(1)] public byte[] CommandEnvelopeContent { get; set; }
    }
}
