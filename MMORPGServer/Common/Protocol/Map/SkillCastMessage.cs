using Common.GameDesign;
using Common.Protocol.Map.Interfaces;
using ReliableUdp.Utility;
using System.Numerics;

namespace Common.Protocol.Map
{
	public class SkillCastMessage : BaseUdpPackage, IPartitionMessage
    {
        public SkillCastType Type { get; set; }

        public string Caster { get; set; } = string.Empty;

        public Vector2 Position { get; set; }

        public long ServerTime { get; set; }

        public SkillTarget Target { get; set; } = new SkillTarget();

        public EntityStats CasterStats { get; set; }

        public SkillCastMessage() : base(MessageType.CastSkill)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(Caster);
            writer.Put(Position.X);
            writer.Put(Position.Y);
            writer.Put(ServerTime);
            writer.Put((byte)Type);
            Target.WriteData(writer);

            if (CasterStats == null)
            {
                writer.Put((byte)0);
            }
            else
            {
                writer.Put((byte)1);
                CasterStats.WriteData(writer);
            }
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            Caster = reader.GetString();
            Position = new Vector2(reader.GetFloat(), reader.GetFloat());
            ServerTime = reader.GetLong();
            Type = (SkillCastType)reader.GetByte();
            Target.ReadData(reader);

            if(reader.GetByte() == 1)
			{
                CasterStats = new EntityStats();
                CasterStats.ReadData(reader);
			}

            return true;
        }
    }
}
