using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Protocol.Chat
{
    public class ChatMessage : IUdpPackage
    {
        public string Message { get; set; }

        public ChatMessage()
        {

        }
        public void Write(UdpDataWriter writer)
        {
            writer.Put((byte)MessageType.Chat);
            writer.Put(Message);
        }

        public bool Read(UdpDataReader reader)
        {
            if (reader.PeekByte() != (byte)MessageType.Chat)
            {
                return false;
            }

            reader.GetByte();

            Message = reader.GetString();

            return true;
        }
    }

}
