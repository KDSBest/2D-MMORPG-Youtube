using ReliableUdp.Utility;

namespace Common.Protocol.Character
{
	public class CharacterMessage : BaseUdpPackage
    {
        public CharacterInformation Character { get; set; } = new CharacterInformation();

        public string Token { get; set; } = string.Empty;

        public CharacterMessage() : base(MessageType.Character)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(Character.Id);
            writer.Put(Character.Name);
            writer.Put(Character.Color);
            writer.Put(Character.Eyes);
            writer.Put(Character.Experience);
            Character.Stats.WriteData(writer);
            writer.Put(Token);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            Character.Id = reader.GetString();
            Character.Name = reader.GetString();
            Character.Color = reader.GetByte();
            Character.Eyes = reader.GetByte();
            Character.Experience = reader.GetInt();
            Character.Stats.ReadData(reader);
            Token = reader.GetString();
            return true;
        }
    }

}
