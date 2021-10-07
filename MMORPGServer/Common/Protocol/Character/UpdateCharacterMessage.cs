using ReliableUdp.Utility;

namespace Common.Protocol.Character
{
	public class UpdateCharacterMessage : BaseUdpPackage
    {
        public UpdateCharacterMessage() : base(MessageType.UpdateCharacter)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            return true;
        }
    }

}
