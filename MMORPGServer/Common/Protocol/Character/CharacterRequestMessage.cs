using ReliableUdp.Utility;
using System.Collections.Generic;

namespace Common.Protocol.Character
{
	public class CharacterRequestMessage : BaseUdpPackage
    {
        public List<string> Names { get; set; } = new List<string>();

        public CharacterRequestMessage() : base(MessageType.CharacterRequest)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            byte count = (byte)Names.Count;
            writer.Put(count);
            for(int i = 0; i < count; i++)
			{
                writer.Put(Names[i]);
			}
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            byte count = reader.GetByte();
            Names = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                Names.Add(reader.GetString());
            }

            return true;
        }
    }

}
