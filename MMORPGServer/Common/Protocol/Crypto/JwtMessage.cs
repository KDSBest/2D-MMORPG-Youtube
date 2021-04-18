using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Text;

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
