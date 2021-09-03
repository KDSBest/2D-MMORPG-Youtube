using Common.GameDesign;
using Common.Protocol.Map.Interfaces;
using ReliableUdp.Utility;
using System.Numerics;

namespace Common.Protocol.Map
{
	public class SkillCastMessage : BaseUdpPackage, IPartitionMessage
    {
        public SkillCastType Type { get; set; }

        public int DurationInMs { get; set; }

        public Vector2 Position { get; set; }

        public long ServerTime { get; set; }

        public SkillCastTargetType TargetType { get; set; }

        public string Target { get; set; }

        public Vector2 TargetPosition { get; set; }

        public SkillCastMessage() : base(MessageType.PropState)
        {
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(Position.X);
            writer.Put(Position.Y);
            writer.Put(ServerTime);
            writer.Put((byte)Type);
            writer.Put(DurationInMs);
            writer.Put((byte)TargetType);

            switch(TargetType)
			{
                case SkillCastTargetType.Position:
                    writer.Put(TargetPosition.X);
                    writer.Put(TargetPosition.Y);
                    break;
                case SkillCastTargetType.Prop:
                    writer.Put(Target);
                    break;
            }
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            Position = new Vector2(reader.GetFloat(), reader.GetFloat());
            ServerTime = reader.GetLong();
            Type = (SkillCastType)reader.GetByte();
            DurationInMs = reader.GetInt();
            TargetType = (SkillCastTargetType)reader.GetByte();

            switch (TargetType)
            {
                case SkillCastTargetType.Position:
                    TargetPosition = new Vector2(reader.GetFloat(), reader.GetFloat());
                    break;
                case SkillCastTargetType.Prop:
                    Target = reader.GetString();
                    break;
            }

            return true;
        }
    }
}
