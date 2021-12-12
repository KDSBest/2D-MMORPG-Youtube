using Common.QuestSystem;
using ReliableUdp.Utility;

namespace Common.Protocol.Quest
{
	public class RequestQuestTracking : BaseUdpPackage
    {
        public RequestQuestTracking() : base(MessageType.ReqQuestTracking)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {

        }

        protected override bool ReadData(UdpDataReader reader)
        {
            return true;
        }
    }

}
