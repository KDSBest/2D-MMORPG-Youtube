using ReliableUdp.Simulation;

namespace ReliableUdp
{
	public class UdpSettings
	{
		public int DisconnectTimeout = 5000;

		public int ReconnectDelay = 5000;

		public int MaxConnectAttempts = 10;

		public bool ReuseAddress = false;

		public INetworkSimulation NetworkSimulation = null;

		public string ConnectKey = "kdsbest";

        public int UpdateSleepTime = 50;
    }
}