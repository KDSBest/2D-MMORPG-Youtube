using System;

using ReliableUdp.BitUtility;
using ReliableUdp.Const;
using ReliableUdp.Enums;
using ReliableUdp.Utility;

namespace ReliableUdp.Packet
{
	public class UdpPacket
	{
		public const int SIZE_LIMIT = ushort.MaxValue - HeaderSize.MAX_UDP;
        public const byte FRAGMENTED_BIT = 0x80;
        public const byte PACKET_TYPE_MASK = 0x7F;

		public PacketType Type
		{
			get { return (PacketType)(this.RawData[0] & PACKET_TYPE_MASK); }
			set { this.RawData[0] = (byte)((this.RawData[0] & FRAGMENTED_BIT) | ((byte)value & PACKET_TYPE_MASK)); }
		}

		public SequenceNumber Sequence
		{
			get { return new SequenceNumber(BitConverter.ToUInt16(this.RawData, 1)); }
			set { BitHelper.Write(this.RawData, 1, (ushort)value.Value); }
		}

		public bool IsFragmented
		{
			get { return (this.RawData[0] & FRAGMENTED_BIT) != 0; }
			set
			{
				if (value)
					this.RawData[0] |= FRAGMENTED_BIT;
				else
					this.RawData[0] &= PACKET_TYPE_MASK;
			}
		}

		public ushort FragmentId
		{
			get { return BitConverter.ToUInt16(this.RawData, 3); }
			set { BitHelper.Write(this.RawData, 3, value); }
		}

		public ushort FragmentPart
		{
			get { return BitConverter.ToUInt16(this.RawData, 5); }
			set { BitHelper.Write(this.RawData, 5, value); }
		}

		public ushort FragmentsTotal
		{
			get { return BitConverter.ToUInt16(this.RawData, 7); }
			set { BitHelper.Write(this.RawData, 7, value); }
		}

		public readonly byte[] RawData;
		public int Size;

		public UdpPacket(int size)
		{
			this.RawData = new byte[size];
			this.Size = 0;
		}

        public UdpPacket(PacketType type, int size)
        {
            size += GetHeaderSize(type);
            this.RawData = new byte[size];
            this.Type = type;
            this.Size = size;
        }

        public static int GetHeaderSize(PacketType type)
		{
            switch (type)
            {
                case PacketType.ReliableOrdered:
                case PacketType.Reliable:
                case PacketType.UnreliableOrdered:
                case PacketType.Ping:
                case PacketType.Pong:
                case PacketType.AckReliable:
                case PacketType.AckReliableOrdered:
                    return HeaderSize.SEQUENCED;
            }

            return HeaderSize.DEFAULT;
        }

        public int GetHeaderSize()
		{
			return GetHeaderSize(this.Type);
		}

		public byte[] GetPacketData()
		{
			int headerSize = GetHeaderSize(this.Type);
			int dataSize = this.Size - headerSize;
			byte[] data = new byte[dataSize];
			Buffer.BlockCopy(this.RawData, headerSize, data, 0, dataSize);
			return data;
		}

		public bool FromBytes(byte[] data, int start, int packetSize)
		{
			byte property = (byte)(data[start] & PACKET_TYPE_MASK);
			bool fragmented = (data[start] & FRAGMENTED_BIT) != 0;
			int headerSize = GetHeaderSize((PacketType)property);

			if (property >= (byte)PacketType.PacketTypeTooHigh ||
				 packetSize > SIZE_LIMIT ||
				 packetSize < headerSize ||
				 (fragmented && packetSize < headerSize + HeaderSize.FRAGMENT))
			{
				return false;
			}

			Buffer.BlockCopy(data, start, this.RawData, 0, packetSize);
			this.Size = packetSize;
			return true;
		}
    }

}
