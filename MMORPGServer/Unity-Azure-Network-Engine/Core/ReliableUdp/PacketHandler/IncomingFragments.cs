using ReliableUdp.Packet;

namespace ReliableUdp.PacketHandler
{
	public class IncomingFragments
	{
		public UdpPacket[] Fragments;
		public int ReceivedCount;
		public int TotalSize;
	}
}