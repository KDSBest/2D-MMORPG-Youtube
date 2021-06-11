using ReliableUdp.Utility;

namespace Common.Protocol.Map
{
	public class RemovePartitionMessage : BaseUdpPackage
    {
        public Vector2Int Partition { get; set; }
        public long ServerTime { get; set; }

        public RemovePartitionMessage() : base(MessageType.RemovePartition)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(Partition.X);
            writer.Put(Partition.Y);
            writer.Put(ServerTime);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            Partition = new Vector2Int(reader.GetInt(), reader.GetInt());
            ServerTime = reader.GetLong();

            return true;
        }
    }
}
