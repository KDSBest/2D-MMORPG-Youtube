using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Protocol.Chat
{
    public class ChatMessage : BaseUdpPackage
    {
        public string Message { get; set; }

        public Guid InstanceId { get; set; }

        public ChatMessage() : base(MessageType.Chat)
        {
            InstanceId = Guid.Empty;
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(InstanceId.ToString());
            writer.Put(Message);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            string guid = reader.GetString();
            InstanceId = new Guid(guid);
            Message = reader.GetString();

            return true;
        }
    }

}
