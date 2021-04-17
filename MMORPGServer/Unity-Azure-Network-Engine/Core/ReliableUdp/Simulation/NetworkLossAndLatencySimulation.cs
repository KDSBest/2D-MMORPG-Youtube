using System;

namespace ReliableUdp.Simulation
{
    public class NetworkLossAndLatencySimulation : INetworkSimulation
	{
		public NetworkPacketLossSimulation PacketLossSimulation { get; set; }

		public NetworkLatencySimulation LatencySimulation { get; set; }

		public NetworkLossAndLatencySimulation(int packetLossPercentage = 10, int minLatencyInMs = 30, int maxLatencyInMs = 100)
		{
			PacketLossSimulation = new NetworkPacketLossSimulation(packetLossPercentage);
			LatencySimulation = new NetworkLatencySimulation(minLatencyInMs, maxLatencyInMs);
		}

		public void Update(Action<byte[], int, UdpEndPoint> dataReceived)
		{
			LatencySimulation.Update(dataReceived);
		}

		public bool HandlePacket(byte[] data, int length, UdpEndPoint endPoint)
		{
			if (!PacketLossSimulation.HandlePacket(data, length, endPoint))
			{
				return false;
			}

			return LatencySimulation.HandlePacket(data, length, endPoint);
		}
	}
}
