using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Protocol.Crypto
{
    public class AESParameterMessage : BaseUdpPackage
    {
        public string AESParameter { get; set; }

        public AESParameterMessage() : base(MessageType.AESParameter)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(AESParameter);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            AESParameter = reader.GetString();

            return true;
        }
    }

}
