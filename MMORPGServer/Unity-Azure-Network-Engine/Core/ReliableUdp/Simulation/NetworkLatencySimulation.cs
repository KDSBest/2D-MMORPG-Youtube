using System;
using System.Collections.Generic;

namespace ReliableUdp.Simulation
{
    public class NetworkLatencySimulation : INetworkSimulation
	{
		private readonly Random randomGenerator = new Random();
		private readonly List<IncomingData> incomingData = new List<IncomingData>();
		private const int TRESHOLD = 5;

		public int SimulationMinLatencyInMs { get; set; }
		public int SimulationMaxLatencyInMs { get; set; }

		public NetworkLatencySimulation(int minLatencyInMs = 30, int maxLatencyInMs = 100)
		{
			this.SimulationMinLatencyInMs = minLatencyInMs;
			this.SimulationMaxLatencyInMs = maxLatencyInMs;
		}

		public void Update(Action<byte[], int, UdpEndPoint> dataReceived)
		{
			var time = DateTime.UtcNow;
			lock (this.incomingData)
			{
				for (int i = 0; i < this.incomingData.Count; i++)
				{
					var incomingData = this.incomingData[i];
					if (incomingData.Time <= time)
					{
						dataReceived(incomingData.Data, incomingData.Data.Length, incomingData.EndPoint);
						this.incomingData.RemoveAt(i);
						i--;
					}
				}
			}
		}

		public bool HandlePacket(byte[] data, int length, UdpEndPoint endPoint)
		{
			int latency = this.randomGenerator.Next(this.SimulationMinLatencyInMs, this.SimulationMaxLatencyInMs);
			if (latency > TRESHOLD)
			{
				byte[] holdedData = new byte[length];
				Buffer.BlockCopy(data, 0, holdedData, 0, length);

				lock (this.incomingData)
				{
					this.incomingData.Add(new IncomingData
					{
						Data = holdedData,
						EndPoint = endPoint,
						Time = DateTime.UtcNow.AddMilliseconds(latency)
					});
				}

				return false;
			}

			return true;
		}
	}
}
