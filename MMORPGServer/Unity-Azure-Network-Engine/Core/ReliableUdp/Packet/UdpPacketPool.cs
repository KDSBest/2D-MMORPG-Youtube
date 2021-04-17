using System;
using System.Collections.Generic;

using ReliableUdp.Const;
using ReliableUdp.Enums;
using ReliableUdp.Utility;

namespace ReliableUdp.Packet
{
	public class UdpPacketPool
	{
        public int PoolLimit = 1000;
		private readonly Stack<UdpPacket> pool;

		public UdpPacketPool()
		{
			this.pool = new Stack<UdpPacket>();
		}

		public UdpPacket GetWithData(PacketType type, UdpDataWriter writer)
		{
			var packet = this.Get(type, writer.Length);
			Buffer.BlockCopy(writer.Data, 0, packet.RawData, UdpPacket.GetHeaderSize(type), writer.Length);
			return packet;
		}

		public UdpPacket GetWithData(PacketType type, byte[] data, int start, int length)
		{
			var packet = this.Get(type, length);
			Buffer.BlockCopy(data, start, packet.RawData, UdpPacket.GetHeaderSize(type), length);
			return packet;
		}

		public UdpPacket GetAndRead(byte[] data, int start, int count)
		{
			UdpPacket packet = null;
			lock (this.pool)
			{
				if (this.pool.Count > 0)
				{
					packet = this.pool.Pop();
				}
			}
			if (packet == null)
			{
				packet = new UdpPacket(Mtu.MaxPacketSize);
			}
			if (!packet.FromBytes(data, start, count))
			{
				this.Recycle(packet);
				return null;
			}
			return packet;
		}

		public UdpPacket Get(PacketType type, int size)
		{
			UdpPacket packet = null;
			size += UdpPacket.GetHeaderSize(type);
			if (size <= Mtu.MaxPacketSize)
			{
				lock (this.pool)
				{
					if (this.pool.Count > 0)
					{
						packet = this.pool.Pop();
					}
				}
			}
			if (packet == null)
			{
				packet = new UdpPacket(size > Mtu.MaxPacketSize ? size : Mtu.MaxPacketSize);
			}
			else
			{
				Array.Clear(packet.RawData, 0, size);
			}

			packet.Type = type;
			packet.Size = size;
			return packet;
		}

		public void Recycle(UdpPacket packet)
		{
			if (packet.Size > Mtu.MaxPacketSize)
			{
				return;
			}

			packet.IsFragmented = false;
			lock (this.pool)
			{
                if(this.pool.Count < PoolLimit)
    				this.pool.Push(packet);
			}
		}
	}
}