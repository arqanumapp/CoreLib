using MessagePack;

namespace CoreLib.Models.Entitys.Message
{
    [MessagePackObject]
    internal class TextMessageContent : MessageContent
    {
        [Key(0)] public string Text { get; set; }
    }
}
