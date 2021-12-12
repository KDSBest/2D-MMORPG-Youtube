using ReliableUdp.Utility;

namespace Common.Protocol.Quest
{
	public class AbbandonQuestMessage : BaseUdpPackage
    {
        public string QuestName;

        public AbbandonQuestMessage() : base(MessageType.AbbandonQuest)
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
