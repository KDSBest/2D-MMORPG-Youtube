using ReliableUdp.Utility;
using ReliableUdp.Enums;
using ReliableUdp.Packet;
using System.Collections.Concurrent;

namespace ReliableUdp.Channel
{
    public class UnreliableOrderedChannel : IUnreliableOrderedChannel
	{
		private SequenceNumber localSequence = new SequenceNumber(0);
		private SequenceNumber remoteSequence = new SequenceNumber(0);
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
            if(!this.outgoingPackets.TryDequeue(out packet))
				return false;

			this.localSequence++;
			packet.Sequence = new SequenceNumber(this.localSequence.Value);
			this.peer.SendRawData(packet);
			this.peer.Recycle(packet);
			return true;
		}

		public void ProcessPacket(UdpPacket packet)
		{
			if ((packet.Sequence - this.remoteSequence) > 0)
			{
				this.remoteSequence = packet.Sequence;
				this.peer.AddIncomingPacket(packet, ChannelType.UnreliableOrdered);
			}
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