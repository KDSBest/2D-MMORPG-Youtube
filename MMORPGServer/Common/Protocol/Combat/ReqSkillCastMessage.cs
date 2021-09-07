using Common.GameDesign;
using ReliableUdp.Utility;
using System.Numerics;

namespace Common.Protocol.Combat
{
	public class ReqSkillCastMessage : BaseUdpPackage
    {
        public SkillCastType Type { get; set; }

        public Vector2 Position { get; set; }

        public SkillTarget Target { get; set; } = new SkillTarget();

        public ReqSkillCastMessage() : base(MessageType.ReqCastSkill)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(Position.X);
            writer.Put(Position.Y);
            writer.Put((byte)Type);
            Target.WriteData(writer);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            Position = new Vector2(reader.GetFloat(), reader.GetFloat());
            Type = (SkillCastType)reader.GetByte();
            Target.ReadData(reader);

            return true;
        }
    }
}
