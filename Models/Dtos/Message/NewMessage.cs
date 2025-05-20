using CoreLib.Models.Entitys.Enums;

namespace CoreLib.Models.Dtos.Message
{
    internal class NewMessage
    {
        public string MessageId { get; set; }

        public string SenderId { get; set; }

        public MessageType MessageType { get; set; }

        public byte[] EncryptedContent { get; set; }

        public byte[] Signature { get; set; }
    }
}
