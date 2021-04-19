using ReliableUdp.Utility;

namespace Common.Protocol.Crypto
{
	public class JwtMessage : BaseUdpPackage
    {
        public string Token { get; set; }

        public JwtMessage() : base(MessageType.Jwt)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(Token);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            Token = reader.GetString();

            return true;
        }
    }

}
