using ReliableUdp.Utility;

namespace Common.Protocol.Quest
{
	public class QuestResultMessage : BaseUdpPackage
    {
        public string Id;
        public bool Result;

        public QuestResultMessage() : base(MessageType.QuestResult)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(Result);

        }

        protected override bool ReadData(UdpDataReader reader)
        {
            Id = reader.GetString();
            Result = reader.GetBool();
            return true;
        }
    }

}
