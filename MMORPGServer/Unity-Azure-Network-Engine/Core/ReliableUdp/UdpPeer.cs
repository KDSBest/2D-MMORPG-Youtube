using System;
using System.Collections.Generic;

using ReliableUdp.NetworkStatistic;
using ReliableUdp.PacketHandler;
using ReliableUdp.Channel;

using ReliableUdp.Enums;
using ReliableUdp.Packet;
using ReliableUdp.Utility;

namespace ReliableUdp
{
    public class UdpPeer
	{
        private readonly UdpPacketPool packetPool;
		private readonly object flushLock = new object();
        private readonly object sendLock = new object();

        public int MaxFlushPacketCount = 1000;

		public UdpEndPoint EndPoint { get; private set; }

		public Dictionary<ChannelType, IChannel> Channels = new Dictionary<ChannelType, IChannel>();

		public NetworkStatisticManagement NetworkStatisticManagement { get; set; }
		public PingPongHandler PacketPingPongHandler { get; set; }
		public MtuHandler PacketMtuHandler { get; set; }
		public MergeHandler PacketMergeHandler { get; set; }
		public ConnectionRequestHandler PacketConnectionRequestHandler { get; set; }
		public FragmentHandler PacketFragmentHandler { get; set; }

        public static readonly int DefaultWindowSize = 64;

        public ConnectionState ConnectionState
		{
			get { return this.PacketConnectionRequestHandler.ConnectionState; }
		}

		public long ConnectId
		{
			get { return this.PacketConnectionRequestHandler.ConnectId; }
		}

        public UdpManager UdpManager { get; }

        public UdpSettings Settings
		{
			get
			{
				return UdpManager.Settings;
			}
		}

		public UdpPeer(UdpManager peerListener, UdpEndPoint endPoint, long connectId)
		{
			this.packetPool = peerListener.PacketPool;
			this.UdpManager = peerListener;
			this.EndPoint = endPoint;

			this.NetworkStatisticManagement = new NetworkStatisticManagement();
			this.PacketPingPongHandler = new PingPongHandler();
			this.PacketMtuHandler = new MtuHandler();
			this.PacketMergeHandler = new MergeHandler();
			this.PacketMergeHandler.Initialize(this);
			this.PacketConnectionRequestHandler = new ConnectionRequestHandler();
			this.PacketConnectionRequestHandler.Initialize(this, connectId);
			this.PacketFragmentHandler = new FragmentHandler();

			this.Channels.Add(ChannelType.Unreliable, new UnreliableUnorderedChannel());
            this.Channels.Add(ChannelType.UnreliableOrdered, new UnreliableOrderedChannel());
			this.Channels.Add(ChannelType.Reliable, new ReliableUnorderedChannel(DefaultWindowSize));
			this.Channels.Add(ChannelType.ReliableOrdered, new ReliableOrderedChannel(DefaultWindowSize));

			foreach (var chan in this.Channels.Values)
			{
				chan.Initialize(this);
			}
		}

		private static PacketType SendOptionsToProperty(ChannelType options)
		{
			switch (options)
			{
				case ChannelType.Reliable:
					return PacketType.Reliable;
				case ChannelType.UnreliableOrdered:
					return PacketType.UnreliableOrdered;
				case ChannelType.ReliableOrdered:
					return PacketType.ReliableOrdered;
				default:
					return PacketType.Unreliable;
			}
		}

		public int GetMaxSinglePacketSize(ChannelType options)
		{
			return this.PacketMtuHandler.Mtu - UdpPacket.GetHeaderSize(SendOptionsToProperty(options));
		}

		public void Send(byte[] data, ChannelType channelType)
		{
			this.Send(data, 0, data.Length, channelType);
		}

		public void Send(UdpDataWriter dataWriter, ChannelType channelType)
		{
			this.Send(dataWriter.Data, 0, dataWriter.Length, channelType);
		}

		public void Send(byte[] data, int start, int length, ChannelType options)
		{
            lock (sendLock)
            {
                PacketType type = SendOptionsToProperty(options);
                int headerSize = UdpPacket.GetHeaderSize(type);

                if (length + headerSize > this.PacketMtuHandler.Mtu)
                {
                    this.PacketFragmentHandler.Send(this, data, start, length, options, headerSize, type);
                    return;
                }

                UdpPacket packet = this.packetPool.GetWithData(type, data, start, length);
                this.SendPacket(packet);
            }
		}

		public void CreateAndSend(PacketType type, SequenceNumber sequence)
		{
			UdpPacket packet = this.packetPool.Get(type, 0);
			packet.Sequence = sequence;
			this.SendPacket(packet);
		}

		public bool SendPacket(UdpPacket packet)
		{
            bool result = true;
#if UDP_DEBUGGING
			System.Diagnostics.Debug.WriteLine($"Packet type {packet.Type}");
#endif
			switch (packet.Type)
			{
				case PacketType.Unreliable:
					this.Channels[ChannelType.Unreliable].AddToQueue(packet);
					break;
				case PacketType.UnreliableOrdered:
					this.Channels[ChannelType.UnreliableOrdered].AddToQueue(packet);
					break;
				case PacketType.Reliable:
					this.Channels[ChannelType.Reliable].AddToQueue(packet);
					break;
				case PacketType.ReliableOrdered:
					this.Channels[ChannelType.ReliableOrdered].AddToQueue(packet);
					break;
				case PacketType.MtuCheck:
					result = this.PacketMtuHandler.SendPacket(this, packet);
					break;
				case PacketType.AckReliable:
				case PacketType.AckReliableOrdered:
				case PacketType.Ping:
				case PacketType.Pong:
				case PacketType.Disconnect:
				case PacketType.MtuOk:
					result = this.SendRawData(packet);
					this.packetPool.Recycle(packet);
					break;
				default:
					throw new Exception("Unknown packet type: " + packet.Type);
			}

            return result;
		}

		public void AddIncomingAck(UdpPacket p, ChannelType channel)
		{
			if (p.IsFragmented)
			{
				this.PacketFragmentHandler.AddIncomingAck(this, p, channel);
			}
			else
			{
				this.UdpManager.ReceiveAckFromPeer(p, this.EndPoint, channel);
				this.packetPool.Recycle(p);
			}
		}

		public void AddIncomingPacket(UdpPacket p, ChannelType channel)
		{
			if (p.IsFragmented)
			{
				this.PacketFragmentHandler.AddIncomingPacket(this, p, channel);
			}
			else
			{
				this.UdpManager.ReceiveFromPeer(p, this.EndPoint, channel);
				this.packetPool.Recycle(p);
			}
		}

		public void ProcessPacket(UdpPacket packet)
		{
			this.NetworkStatisticManagement.ResetTimeSinceLastPacket();

#if UDP_DEBUGGING
			System.Diagnostics.Debug.WriteLine($"Packet type {packet.Type}");
#endif
            switch (packet.Type)
			{
				case PacketType.ConnectAccept:
					this.PacketConnectionRequestHandler.ProcessAcceptPacket(this, packet);
					break;
				case PacketType.ConnectRequest:
					this.PacketConnectionRequestHandler.ProcessPacket(this, packet);
					break;
				case PacketType.Merged:
					this.PacketMergeHandler.ProcessPacket(this, packet);
					break;
				case PacketType.Ping:
					this.PacketPingPongHandler.HandlePing(this, packet);
					break;
				case PacketType.Pong:
					this.PacketPingPongHandler.HandlePong(this, packet);
					break;
				case PacketType.AckReliable:
					this.Channels[ChannelType.Reliable].ProcessAck(packet);
					this.packetPool.Recycle(packet);
					break;
				case PacketType.AckReliableOrdered:
					this.Channels[ChannelType.ReliableOrdered].ProcessAck(packet);
					this.packetPool.Recycle(packet);
					break;
				case PacketType.UnreliableOrdered:
					this.Channels[ChannelType.UnreliableOrdered].ProcessPacket(packet);
					break;
				case PacketType.Reliable:
					this.Channels[ChannelType.Reliable].ProcessPacket(packet);
					break;
				case PacketType.ReliableOrdered:
					this.Channels[ChannelType.ReliableOrdered].ProcessPacket(packet);
					break;
				case PacketType.Unreliable:
					this.Channels[ChannelType.Unreliable].ProcessPacket(packet);
					return;
				case PacketType.MtuCheck:
				case PacketType.MtuOk:
					this.PacketMtuHandler.ProcessMtuPacket(this, packet);
					break;

				default:
#if UDP_DEBUGGING
					System.Diagnostics.Debug.WriteLine($"Error! Unexpected packet type {packet.Type}");
#endif
					break;
			}
		}

		public bool SendRawData(UdpPacket packet)
		{
			if (this.PacketMergeHandler.SendRawData(this, packet))
			{
				return true;
			}

#if UDP_DEBUGGING
			System.Diagnostics.Debug.WriteLine($"Sending Packet {packet.Type}");
#endif
			return this.UdpManager.SendRaw(packet.RawData, 0, packet.Size, this.EndPoint);
		}

		public bool SendRaw(byte[] message, int start, int length, UdpEndPoint endPoint)
		{
			return this.UdpManager.SendRaw(message, start, length, endPoint);
		}

		public void Flush()
		{
			lock (this.flushLock)
			{
                int currentSended = 0;
                while (currentSended < MaxFlushPacketCount)
                {
                    if (this.Channels[ChannelType.ReliableOrdered].SendNextPacket() ||
                         this.Channels[ChannelType.Reliable].SendNextPacket() ||
                         this.Channels[ChannelType.UnreliableOrdered].SendNextPacket() ||
                         this.Channels[ChannelType.Unreliable].SendNextPacket())
                    {
                        currentSended++;
                    }
                    else
                    {
                        break;
                    }
                }

                this.NetworkStatisticManagement.FlowManagement.IncreaseSendedPacketCount(currentSended);

                this.PacketMergeHandler.SendQueuedPackets(this);
            }
        }

		public void Update(int deltaTime)
		{
			if (!this.PacketConnectionRequestHandler.Update(this, deltaTime))
				return;

			int currentMaxSend = this.NetworkStatisticManagement.FlowManagement.GetCurrentMaxSend(deltaTime);

			foreach (var chan in this.Channels.Values)
			{
				chan.SendAcks();
			}

			this.NetworkStatisticManagement.Update(this, deltaTime, this.UdpManager.ConnectionLatencyUpdated);
			this.PacketPingPongHandler.Update(this, deltaTime);

			this.PacketMtuHandler.Update(this, deltaTime);

            this.Flush();
		}

		public void Recycle(UdpPacket packet)
		{
			this.packetPool.Recycle(packet);
		}

		public UdpPacket GetPacketFromPool(PacketType type, int bytesCount)
		{
			return this.packetPool.Get(type, bytesCount);
		}

		public bool SendRawAndRecycle(UdpPacket packet, UdpEndPoint peerEndPoint)
		{
			return this.UdpManager.SendRawAndRecycle(packet, peerEndPoint);
		}

		public UdpPacket GetAndRead(byte[] packetRawData, int pos, ushort size)
		{
			return this.packetPool.GetAndRead(packetRawData, pos, size);
		}

		public void EnqueueEvent(UdpEvent evt)
		{
			this.UdpManager.EnqueueEvent(evt);
		}
	}
}
