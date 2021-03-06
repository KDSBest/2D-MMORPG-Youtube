using ReliableUdp.Utility;

namespace Common.Protocol.Crypto
{
	public class RSAPublicKeyMessage : BaseUdpPackage
    {
        public string PublicKey { get; set; }

        public RSAPublicKeyMessage() : base(MessageType.RSAPublicKey)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(PublicKey);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            PublicKey = reader.GetString();

            return true;
        }
    }

}
