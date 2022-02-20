using Common.GameDesign;
using Common.Protocol.Map.Interfaces;
using ReliableUdp.Utility;
using System;
using System.Numerics;

namespace Common.Protocol.Map
{
	public class PropStateMessage : BaseUdpPackage, IMapStateMessage<PropStateMessage>
    {
        public PropType Type { get; set; }
        public string Name { get; set; } = string.Empty;

        public Vector2 Position { get; set; }
        public bool IsLookingRight { get; set; }
        public int Animation { get; set; }
        public long ServerTime { get; set; }

        public EntityStats Stats { get; set; } = new EntityStats();

        public PropStateMessage() : base(MessageType.PropState)
        {
        }

        public bool HasNoVisibleDifference(PropStateMessage msg)
        {
            return Math.Abs(this.Position.X - msg.Position.X) < MapConfiguration.SmallDistance
                                && Math.Abs(this.Position.Y - msg.Position.Y) < MapConfiguration.SmallDistance
                                && this.IsLookingRight == msg.IsLookingRight
                                && this.Animation == msg.Animation
                                && this.Stats.HP == msg.Stats.HP
                                && this.Stats.MaxHP == msg.Stats.MaxHP;
        }

        protected override void WriteData(UdpDataWriter writer)
        {
            writer.Put(Name);
            writer.Put(Position.X);
            writer.Put(Position.Y);
            writer.Put(IsLookingRight);
            writer.Put(Animation);
            writer.Put(ServerTime);
            writer.Put((byte)Type);
            Stats.WriteData(writer);
        }

        protected override bool ReadData(UdpDataReader reader)
        {
            Name = reader.GetString();
            Position = new Vector2(reader.GetFloat(), reader.GetFloat());
            IsLookingRight = reader.GetBool();
            Animation = reader.GetInt();
            ServerTime = reader.GetLong();
            Type = (PropType)reader.GetByte();
            Stats.ReadData(reader);

            return true;
        }
    }
}
