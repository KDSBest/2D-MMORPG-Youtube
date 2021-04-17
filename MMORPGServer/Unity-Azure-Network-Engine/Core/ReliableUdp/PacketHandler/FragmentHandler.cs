using System;
using System.Collections.Generic;

using ReliableUdp.Const;
using ReliableUdp.Enums;
using ReliableUdp.Packet;

namespace ReliableUdp.PacketHandler
{
    public class FragmentHandler
	{
		private ushort fragmentId;
		private readonly Dictionary<ushort, IncomingFragments> holdedFragments = new Dictionary<ushort, IncomingFragments>();
		private readonly Dictionary<ushort, IncomingFragments> holdedFragmentsForAck = new Dictionary<ushort, IncomingFragments>();

		public void AddIncomingAck(UdpPeer peer, UdpPacket p, ChannelType channel)
		{
#if UDP_DEBUGGING
			System.Diagnostics.Debug.WriteLine($"Fragment. Id: {p.FragmentId}, Part: {p.FragmentPart}, Total: {p.FragmentsTotal}");
#endif

			ushort packetFragId = p.FragmentId;
			IncomingFragments incomingFragments;
			if (!this.holdedFragmentsForAck.TryGetValue(packetFragId, out incomingFragments))
			{
				incomingFragments = new IncomingFragments
				{
					Fragments = new UdpPacket[p.FragmentsTotal]
				};
				this.holdedFragmentsForAck.Add(packetFragId, incomingFragments);
			}

			var fragments = incomingFragments.Fragments;

			if (p.FragmentPart >= fragments.Length || fragments[p.FragmentPart] != null)
			{
#if UDP_DEBUGGING
				System.Diagnostics.Debug.WriteLine($"Invalid fragment packet.");
#endif
				return;
			}

			fragments[p.FragmentPart] = p;

			incomingFragments.ReceivedCount++;

			int dataOffset = p.GetHeaderSize() + HeaderSize.FRAGMENT;
			incomingFragments.TotalSize += p.Size - dataOffset;

			if (incomingFragments.ReceivedCount != fragments.Length)
			{
				return;
			}

#if UDP_DEBUGGING
			System.Diagnostics.Debug.WriteLine($"Received all fragments.");
#endif
			UdpPacket resultingPacket = peer.GetPacketFromPool(p.Type, incomingFragments.TotalSize);

			int resultingPacketOffset = resultingPacket.GetHeaderSize();
			int firstFragmentSize = fragments[0].Size - dataOffset;
			for (int i = 0; i < incomingFragments.ReceivedCount; i++)
			{
				int fragmentSize = fragments[i].Size - dataOffset;
				Buffer.BlockCopy(
					 fragments[i].RawData,
					 dataOffset,
					 resultingPacket.RawData,
					 resultingPacketOffset + firstFragmentSize * i,
					 fragmentSize);

				peer.Recycle(fragments[i]);
				fragments[i] = null;
			}

			peer.UdpManager.ReceiveAckFromPeer(resultingPacket, peer.EndPoint, channel);

			peer.Recycle(resultingPacket);
			this.holdedFragmentsForAck.Remove(packetFragId);
		}

		public void AddIncomingPacket(UdpPeer peer, UdpPacket p, ChannelType channel)
		{
#if UDP_DEBUGGING
			System.Diagnostics.Debug.WriteLine($"Fragment. Id: {p.FragmentId}, Part: {p.FragmentPart}, Total: {p.FragmentsTotal}");
#endif
			ushort packetFragId = p.FragmentId;
			IncomingFragments incomingFragments;
			if (!this.holdedFragments.TryGetValue(packetFragId, out incomingFragments))
			{
				incomingFragments = new IncomingFragments { Fragments = new UdpPacket[p.FragmentsTotal] };
				this.holdedFragments.Add(packetFragId, incomingFragments);
			}

			var fragments = incomingFragments.Fragments;

			if (p.FragmentPart >= fragments.Length || fragments[p.FragmentPart] != null)
			{
				peer.Recycle(p);
#if UDP_DEBUGGING
				System.Diagnostics.Debug.WriteLine($"Invalid fragment packet.");
#endif
                return;
			}

			fragments[p.FragmentPart] = p;

			incomingFragments.ReceivedCount++;

			int dataOffset = p.GetHeaderSize() + HeaderSize.FRAGMENT;
			incomingFragments.TotalSize += p.Size - dataOffset;

			if (incomingFragments.ReceivedCount != fragments.Length)
			{
				return;
			}

#if UDP_DEBUGGING
			System.Diagnostics.Debug.WriteLine($"Received all fragments.");
#endif
			UdpPacket resultingPacket = peer.GetPacketFromPool(p.Type, incomingFragments.TotalSize);

			int resultingPacketOffset = resultingPacket.GetHeaderSize();
			int firstFragmentSize = fragments[0].Size - dataOffset;
			for (int i = 0; i < incomingFragments.ReceivedCount; i++)
			{
				int fragmentSize = fragments[i].Size - dataOffset;
				Buffer.BlockCopy(fragments[i].RawData, dataOffset, resultingPacket.RawData, resultingPacketOffset + firstFragmentSize * i, fragmentSize);

				peer.Recycle(fragments[i]);
				fragments[i] = null;
			}

			peer.UdpManager.ReceiveFromPeer(resultingPacket, peer.EndPoint, channel);

			peer.Recycle(resultingPacket);
			this.holdedFragments.Remove(packetFragId);
		}

		public void Send(UdpPeer peer, byte[] data, int start, int length, ChannelType options, int headerSize, PacketType type)
		{
			if (options == ChannelType.UnreliableOrdered || options == ChannelType.Unreliable)
			{
				throw new Exception("Unreliable packet size > allowed (" + (peer.PacketMtuHandler.Mtu - headerSize) + ")");
			}

			int packetFullSize = peer.PacketMtuHandler.Mtu - headerSize;
			int packetDataSize = packetFullSize - HeaderSize.FRAGMENT;

			int fullPacketsCount = length / packetDataSize;
			int lastPacketSize = length % packetDataSize;
			int totalPackets = fullPacketsCount + (lastPacketSize == 0 ? 0 : 1);

#if UDP_DEBUGGING
            System.Diagnostics.Debug.WriteLine($"FragmentSend:\r\n" +
                          $" MTU: {peer.PacketMtuHandler.Mtu}\n" +
                          $" headerSize: {headerSize}\n" +
                          $" packetFullSize: {packetFullSize}\n" +
                          $" packetDataSize: {packetDataSize}\n" +
                          $" fullPacketsCount: {fullPacketsCount}\n" +
                          $" lastPacketSize: {lastPacketSize}\n" +
                          $" totalPackets: {totalPackets}");
#endif

            if (totalPackets > ushort.MaxValue)
			{
				throw new Exception("Too many fragments: " + totalPackets + " > " + ushort.MaxValue);
			}

			int dataOffset = headerSize + HeaderSize.FRAGMENT;
			for (ushort i = 0; i < fullPacketsCount; i++)
			{
				UdpPacket p = peer.GetPacketFromPool(type, packetFullSize);
				p.FragmentId = this.fragmentId;
				p.FragmentPart = i;
				p.FragmentsTotal = (ushort)totalPackets;
				p.IsFragmented = true;
				Buffer.BlockCopy(data, i * packetDataSize, p.RawData, dataOffset, packetDataSize);
				peer.SendPacket(p);
			}

			if (lastPacketSize > 0)
			{
				UdpPacket p = peer.GetPacketFromPool(type, lastPacketSize + HeaderSize.FRAGMENT);
				p.FragmentId = this.fragmentId;
				p.FragmentPart = (ushort)fullPacketsCount;
				p.FragmentsTotal = (ushort)totalPackets;
				p.IsFragmented = true;
				Buffer.BlockCopy(data, fullPacketsCount * packetDataSize, p.RawData, dataOffset, lastPacketSize);
				peer.SendPacket(p);
			}

			fragmentId++;
		}
	}
}
