using ReliableUdp.Utility;

namespace Common.Protocol.Login
{
	public class RegisterMessage : BaseUdpPackage
    {
        public string EMailEnc { get; set; }
        public string PasswordEnc { get; set; }

        public RegisterMessage() : base(MessageType.Register)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(EMailEnc);
            writer.Put(PasswordEnc);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            EMailEnc = reader.GetString();
            PasswordEnc = reader.GetString();

            return true;
        }
    }

}
