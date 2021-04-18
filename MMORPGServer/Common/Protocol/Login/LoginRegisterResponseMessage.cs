using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Protocol.Login
{

	public class LoginRegisterResponseMessage : BaseUdpPackage
    {
        public LoginRegisterResponse Response { get; set; }

        public string Token { get; set; } = string.Empty;

        public LoginRegisterResponseMessage() : base(MessageType.LoginRegisterResponse)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put((byte)Response);
            writer.Put(Token);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            Response = (LoginRegisterResponse)reader.GetByte();
            Token = reader.GetString();

            return true;
        }
    }

}
