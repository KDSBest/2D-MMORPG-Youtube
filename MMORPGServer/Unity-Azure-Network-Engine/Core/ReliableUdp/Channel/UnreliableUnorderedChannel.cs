using ReliableUdp.Enums;
using ReliableUdp.Packet;
using System.Collections.Concurrent;

namespace ReliableUdp.Channel
{
    public class UnreliableUnorderedChannel : IUnreliableChannel
	{
		private readonly ConcurrentQueue<UdpPacket> outgoingPackets = new ConcurrentQueue<UdpPacket>();
		private UdpPeer peer;

		public void Initialize(UdpPeer peer)
		{
			this.peer = peer;
		}

		public void AddToQueue(UdpPacket packet)
		{
			this.outgoingPackets.Enqueue(packet);
		}

		public bool SendNextPacket()
		{
			UdpPacket packet = null;

			if (!this.outgoingPackets.TryDequeue(out packet))
				return false;

			this.peer.SendRawData(packet);
			this.peer.Recycle(packet);

			return true;
		}

		public void ProcessPacket(UdpPacket packet)
		{
			this.peer.AddIncomingPacket(packet, ChannelType.Unreliable);
		}

		public int PacketsInQueue
		{
			get
			{
				return 0;
			}
		}

		public void ProcessAck(UdpPacket packet)
		{
		}

		public void SendAcks()
		{
		}
	}
}
