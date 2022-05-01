using ReliableUdp.Utility;
using System.Numerics;

namespace Common.GameDesign
{
	public class SkillTarget
	{
		public SkillCastTargetType TargetType { get; set; }

		public string TargetName { get; set; }

		public Vector2 TargetPosition { get; set; }

        public void WriteData(UdpDataWriter writer)
        {
            writer.Put((byte)TargetType);

            switch (TargetType)
            {
                case SkillCastTargetType.Position:
                    writer.Put(TargetPosition.X);
                    writer.Put(TargetPosition.Y);
                    writer.Put(TargetName);
                    break;
                case SkillCastTargetType.SingleTarget:
                    writer.Put(TargetName);
                    break;
            }
        }

        public void ReadData(UdpDataReader reader)
        {
            TargetType = (SkillCastTargetType)reader.GetByte();

            switch (TargetType)
            {
                case SkillCastTargetType.Position:
                    TargetPosition = new Vector2(reader.GetFloat(), reader.GetFloat());
                    TargetName = reader.GetString();
                    break;
                case SkillCastTargetType.SingleTarget:
                    TargetName = reader.GetString();
                    break;
            }
        }
    }
}
