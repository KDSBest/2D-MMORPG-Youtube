using ReliableUdp.Utility;

namespace Common.Protocol.Quest
{
	public class AcceptQuestMessage : BaseUdpPackage
    {
        public string QuestName;

        public AcceptQuestMessage() : base(MessageType.AcceptQuest)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(QuestName);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            QuestName = reader.GetString();
            return true;
        }
    }

}
