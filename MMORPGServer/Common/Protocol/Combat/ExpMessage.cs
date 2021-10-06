using Common.GameDesign;
using ReliableUdp.Utility;
using System.Numerics;

namespace Common.Protocol.Combat
{
	public class ExpMessage : BaseUdpPackage
    {
        public int ExpGain = 5;

        public ExpMessage() : base(MessageType.Exp)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(ExpGain);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            ExpGain = reader.GetInt();

            return true;
        }
    }
}
