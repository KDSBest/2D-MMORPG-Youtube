using ReliableUdp.Utility;

namespace Common.Protocol.Map
{
	public class RemoveStateMessage : BaseUdpPackage
    {
        public string Name { get; set; } = string.Empty;
        public long ServerTime { get; set; }

        public RemoveStateMessage() : base(MessageType.RemoveState)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(Name);
            writer.Put(ServerTime);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            Name = reader.GetString();
            ServerTime = reader.GetLong();

            return true;
        }
    }
}
