using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Models.Entitys.Chat
{
    internal class Chat
    {
        public string Id { get; set; }
        public string PeerAccountId { get; set; }
        public string PeerDisplayName { get; set; }
        public ChatKey LocalKey { get; set; }
        public List<PeerKeys> RemoteKey { get; set; }
        public List<Message> Messages { get; set; }
    }
    internal class ChatKey
    {
        public string Id { get; set; }
        public string PK { get; set; } //MlKemKey
        public string PrK { get; set; } //MLDsaKey
    }
    internal class PeerKeys
    {
        public string Id { get; set; }
        public string PK { get; set; } //MlKemKey
        public string SPK { get; set; } //MLDsaKey
    }
    internal class Message
    {
        public string Id { get; set; }
        public string Content { get; set; } //Message content
        public DateTime Timestamp { get; set; }
    }
}
