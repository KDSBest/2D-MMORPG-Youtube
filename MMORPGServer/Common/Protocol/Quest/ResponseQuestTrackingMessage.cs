using Common.QuestSystem;
using ReliableUdp.Utility;

namespace Common.Protocol.Quest
{
	public class ResponseQuestTrackingMessage : BaseUdpPackage
    {
        public QuestTracking QuestTracking;

        public ResponseQuestTrackingMessage() : base(MessageType.ResponseQuestTracking)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(QuestTracking.QuestTrackingValues.Count);
            foreach(var kv in QuestTracking.QuestTrackingValues)
			{
                writer.Put(kv.Key);
                writer.Put(kv.Value);
			}

            writer.Put(QuestTracking.AcceptedQuests.ToArray());
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            QuestTracking = new QuestTracking();
            int c = reader.GetInt();
            for(int i = 0; i < c; i++)
			{
                QuestTracking.QuestTrackingValues.Add(reader.GetString(), reader.GetInt());
			}

            QuestTracking.AcceptedQuests.Clear();
            QuestTracking.AcceptedQuests.AddRange(reader.GetStringArray(1000));

            return true;
        }
    }

}
