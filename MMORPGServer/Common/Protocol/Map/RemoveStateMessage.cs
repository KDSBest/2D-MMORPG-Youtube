using ReliableUdp.Utility;

namespace Common.Protocol.Map
{
	public class RemoveStateMessage : BaseUdpPackage
    {
        public string Name { get; set; } = string.Empty;
        public long ServerTime { get; set; }
        public Vector2Int Partition { get; set; }

        public RemoveStateMessage() : base(MessageType.RemoveState)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(Name);
            writer.Put(ServerTime);
            writer.Put(Partition.X);
            writer.Put(Partition.Y);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            Name = reader.GetString();
            ServerTime = reader.GetLong();
            Partition = new Vector2Int(reader.GetInt(), reader.GetInt());
            return true;
        }
    }
}
