using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Protocol.Chat
{
    public class ChatMessage : BaseUdpPackage
    {
        public string Sender { get; set; } = string.Empty;
        public string Message { get; set; }

        public ChatMessage() : base(MessageType.Chat)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(Message);
            writer.Put(Sender);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            Message = reader.GetString();
            Sender = reader.GetString();
            return true;
        }
    }

}
