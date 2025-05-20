using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Models.Entitys.Message
{
    [MessagePackObject]
    internal class ReplyMessageContent : TextMessageContent
    {
        [Key(1)] public string RepliedToMessageId { get; set; }
        [Key(2)] public string RepliedPreviewText { get; set; }
    }
}
