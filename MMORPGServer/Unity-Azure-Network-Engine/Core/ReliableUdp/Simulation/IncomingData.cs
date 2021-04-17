using System;

namespace ReliableUdp.Simulation
{
    public struct IncomingData
	{
		public byte[] Data;
		public UdpEndPoint EndPoint;
		public DateTime Time;
	}

}
