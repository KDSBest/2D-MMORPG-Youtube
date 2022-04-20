using ReliableUdp.Utility;

namespace Common.Protocol.Login
{
	public class LoginMessage : BaseUdpPackage
    {
        public string EMail { get; set; }
        public string Password { get; set; }

        public LoginMessage() : base(MessageType.Login)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(EMail);
            writer.Put(Password);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            EMail = reader.GetString();
            Password = reader.GetString();

            return true;
        }
    }

}
