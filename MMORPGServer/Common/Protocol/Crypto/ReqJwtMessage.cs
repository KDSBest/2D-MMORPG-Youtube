using ReliableUdp.Utility;

namespace Common.Protocol.Crypto
{
	public class ReqJwtMessage : BaseUdpPackage
    {
        public ReqJwtMessage() : base(MessageType.ReqJwt)
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
