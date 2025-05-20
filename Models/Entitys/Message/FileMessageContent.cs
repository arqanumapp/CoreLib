using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Models.Entitys.Message
{
    [MessagePackObject]
    internal class FileMessageContent : MessageContent
    {
        [Key(0)] public string FileName { get; set; }
        [Key(1)] public string MimeType { get; set; }
        [Key(2)] public long FileSize { get; set; }
        [Key(3)] public byte[] EncryptedFileKey { get; set; } // Ключ для расшифровки
        [Key(4)] public string RemoteUrl { get; set; } // Если хранится удалённо
    }
}
