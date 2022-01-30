using ReliableUdp.Utility;

namespace Common.Protocol.Map
{
	public class TeleportMessage : BaseUdpPackage
	{
		public string Name { get; set; } = string.Empty;

		public TeleportMessage() : base(MessageType.Teleport)
		{
		}

		protected override void WriteData(UdpDataWriter writer)
		{
			writer.Put(Name);
		}

		protected override bool ReadData(UdpDataReader reader)
		{
			Name = reader.GetString();

			return true;
		}
	}
}
