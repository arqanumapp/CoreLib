using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Models.Entitys.Message
{
    [MessagePackObject]
    [Union(0, typeof(TextMessageContent))]
    [Union(1, typeof(ImageMessageContent))]
    [Union(2, typeof(VideoMessageContent))]
    [Union(3, typeof(AudioMessageContent))]
    [Union(4, typeof(FileMessageContent))]
    [Union(5, typeof(ReplyMessageContent))]
    internal abstract class MessageContent
    {
        // Базовый класс – не содержит полей
    }
}
