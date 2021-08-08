using Newtonsoft.Json;
using ReliableUdp.Utility;
using System;

namespace Common.Protocol.PlayerEvent
{
	public class PlayerEventMessage : BaseUdpPackage
    {
        public PlayerEventType Type { get; set; }
        public DateTime CreationDate { get; set; }

        public PlayerEventMessage() : base(MessageType.PlayerEvent)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put((byte)Type);
            writer.Put(CreationDate.Ticks);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            Type = (PlayerEventType)reader.GetByte();
            CreationDate = new DateTime(reader.GetLong());

            return true;
        }
    }
}
