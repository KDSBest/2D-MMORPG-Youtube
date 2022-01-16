using ReliableUdp.Utility;

namespace Common.Protocol.Quest
{
	public class FinishQuestMessage : BaseUdpPackage
    {
        public string QuestName;

        public FinishQuestMessage() : base(MessageType.FinishQuest)
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
