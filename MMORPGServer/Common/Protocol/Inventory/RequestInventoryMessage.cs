using ReliableUdp.Utility;

namespace Common.Protocol.Inventory
{
	public class RequestInventoryMessage : BaseUdpPackage
    {
        public RequestInventoryMessage() : base(MessageType.ReqInventory)
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
