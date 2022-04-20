using ReliableUdp.Utility;

namespace Common.Protocol.Login
{
	public class RegisterMessage : BaseUdpPackage
    {
        public string EMail { get; set; }
        public string Password { get; set; }

        public RegisterMessage() : base(MessageType.Register)
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
