using Common.Protocol.Map.Interfaces;
using Newtonsoft.Json;
using ReliableUdp.Utility;
using System;
using System.Numerics;

namespace Common.Protocol.Map
{
	public class PlayerStateMessage : BaseUdpPackage, IMapStateMessage<PlayerStateMessage>
	{
		[JsonProperty(PropertyName = "id")]
		public string Name { get; set; } = string.Empty;

		public Vector2 Position { get; set; }
		public bool IsLookingRight { get; set; }
		public int Animation { get; set; }
		public long ServerTime { get; set; }
		public bool ForcePosition { get; set; } = false;

		public PlayerStateMessage() : base(MessageType.PlayerState)
		{
		}

		public bool HasNoVisibleDifference(PlayerStateMessage msg)
		{
			return Math.Abs(this.Position.X - msg.Position.X) < MapConfiguration.SmallDistance
								&& Math.Abs(this.Position.Y - msg.Position.Y) < MapConfiguration.SmallDistance
								&& this.IsLookingRight == msg.IsLookingRight
								&& this.Animation == msg.Animation;
		}

		protected override void WriteData(UdpDataWriter writer)
		{
			writer.Put(Name);
			writer.Put(Position.X);
			writer.Put(Position.Y);
			writer.Put(IsLookingRight);
			writer.Put(Animation);
			writer.Put(ServerTime);
			writer.Put(ForcePosition);
		}

		protected override bool ReadData(UdpDataReader reader)
		{
			Name = reader.GetString();
			Position = new Vector2(reader.GetFloat(), reader.GetFloat());
			IsLookingRight = reader.GetBool();
			Animation = reader.GetInt();
			ServerTime = reader.GetLong();
			ForcePosition = reader.GetBool();

			return true;
		}

		public PlayerStateMessage Clone()
		{
			return new PlayerStateMessage()
			{
				Name = Name,
				Animation = Animation,
				ForcePosition = ForcePosition,
				IsLookingRight = IsLookingRight,
				Position = Position,
				ServerTime = ServerTime
			};
		}
	}
}
