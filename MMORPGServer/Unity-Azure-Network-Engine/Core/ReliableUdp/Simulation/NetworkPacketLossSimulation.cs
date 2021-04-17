using System;

namespace ReliableUdp.Simulation
{
    public class NetworkPacketLossSimulation : INetworkSimulation
	{
		public int PacketLossChance { get; set; }

		private readonly Random randomGenerator = new Random();

		public NetworkPacketLossSimulation(int packetLossPercentage = 10)
		{
			PacketLossChance = packetLossPercentage;
		}

		public void Update(Action<byte[], int, UdpEndPoint> dataReceived)
		{
			
		}

		public bool HandlePacket(byte[] data, int length, UdpEndPoint endPoint)
		{
			return this.randomGenerator.Next(100 / PacketLossChance) != 0;
		}
	}
}
