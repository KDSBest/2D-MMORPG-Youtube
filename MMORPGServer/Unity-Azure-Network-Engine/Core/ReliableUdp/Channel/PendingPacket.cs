using System;

using ReliableUdp.Packet;

namespace ReliableUdp.Channel
{
	public class PendingPacket
	{
		public UdpPacket Packet;
		public DateTime? TimeStamp;

		public UdpPacket GetAndClear()
		{
			var packet = this.Packet;
			this.Packet = null;
			this.TimeStamp = null;
			return packet;
		}
	}
}