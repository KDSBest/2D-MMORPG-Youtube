using System;

using ReliableUdp.BitUtility;
using ReliableUdp.Const;
using ReliableUdp.Enums;
using ReliableUdp.Packet;

namespace ReliableUdp.PacketHandler
{
    public class MergeHandler
	{
		public bool Enabled { get; set; }
		private UdpPacket mergeData;
		private int mergePos;
		private int mergeCount;

		public MergeHandler()
		{
			Enabled = true;
		}

		private static bool CanMerge(PacketType type)
		{
			switch (type)
			{
				case PacketType.ConnectAccept:
				case PacketType.ConnectRequest:
				case PacketType.MtuOk:
				case PacketType.Pong:
				case PacketType.Disconnect:
					return false;
				default:
					return true;
			}
		}

		public void Initialize(UdpPeer peer)
		{
			this.mergeData = peer.GetPacketFromPool(PacketType.Merged, Mtu.MaxPacketSize);
		}

		public bool SendRawData(UdpPeer peer, UdpPacket packet)
		{
			if (Enabled &&
				 CanMerge(packet.Type) &&
				 this.mergePos + packet.Size + HeaderSize.DEFAULT * 2 + 2 < peer.PacketMtuHandler.Mtu)
			{
				BitHelper.Write(this.mergeData.RawData, this.mergePos + HeaderSize.DEFAULT, (ushort)packet.Size);
				Buffer.BlockCopy(packet.RawData, 0, this.mergeData.RawData, this.mergePos + HeaderSize.DEFAULT + 2, packet.Size);
				this.mergePos += packet.Size + 2;
				this.mergeCount++;

#if UDP_DEBUGGING
                System.Diagnostics.Debug.WriteLine($"Merged: {mergePos}/{peer.PacketMtuHandler.Mtu - 2}, count: {mergeCount}");
#endif
                return true;
			}

			return false;
		}

		public void SendQueuedPackets(UdpPeer peer)
		{
			if (this.mergePos > 0)
			{
				if (this.mergeCount > 1)
				{
#if UDP_DEBUGGING
					System.Diagnostics.Debug.WriteLine($"Send merged {this.mergePos}, count {this.mergeCount}");
#endif
                    peer.SendRaw(this.mergeData.RawData, 0, HeaderSize.DEFAULT + this.mergePos, peer.EndPoint);
				}
				else
				{
					peer.SendRaw(this.mergeData.RawData, HeaderSize.DEFAULT + 2, this.mergePos - 2, peer.EndPoint);
				}
				this.mergePos = 0;
				this.mergeCount = 0;
			}
		}

		public void ProcessPacket(UdpPeer peer, UdpPacket packet)
		{
			int pos = HeaderSize.DEFAULT;
			while (pos < packet.Size)
			{
				ushort size = BitConverter.ToUInt16(packet.RawData, pos);
				pos += 2;
				UdpPacket mergedPacket = peer.GetAndRead(packet.RawData, pos, size);
				if (mergedPacket == null)
				{
					peer.Recycle(packet);
					break;
				}

				pos += size;
				peer.ProcessPacket(mergedPacket);
			}
		}
	}
}
