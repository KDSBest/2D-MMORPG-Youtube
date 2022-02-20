using Common.GameDesign;
using ReliableUdp.Utility;

namespace Common.Protocol.Character
{
	public class UpdateCharacterMessage : BaseUdpPackage
    {
        public string Name;
        public EntityStats Stats = new EntityStats();

        public UpdateCharacterMessage() : base(MessageType.UpdateCharacter)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(Name);
            Stats.WriteData(writer);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            Name = reader.GetString();
            Stats.ReadData(reader);
            return true;
        }
    }

}
