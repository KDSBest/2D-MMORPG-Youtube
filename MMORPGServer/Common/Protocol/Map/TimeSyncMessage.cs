using ReliableUdp.Utility;

namespace Common.Protocol.Map
{
	public class TimeSyncMessage : BaseUdpPackage
    {
        public long MyTime { get; set; }
        public long ServerTime { get; set; }

        public TimeSyncMessage() : base(MessageType.TimeSync)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(MyTime);
            writer.Put(ServerTime);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            MyTime = reader.GetLong();
            ServerTime = reader.GetLong();

            return true;
        }
    }
}
