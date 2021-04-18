using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Protocol.Login
{
    public class LoginMessage : BaseUdpPackage
    {
        public string EMailEnc { get; set; }
        public string PasswordEnc { get; set; }

        public LoginMessage() : base(MessageType.Login)
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
