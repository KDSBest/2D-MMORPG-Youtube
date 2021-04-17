using System;

namespace ReliableUdp.Simulation
{
	public interface INetworkSimulation
	{
		void Update(Action<byte[], int, UdpEndPoint> dataReceived);

		bool HandlePacket(byte[] data, int length, UdpEndPoint endPoint);
	}
}