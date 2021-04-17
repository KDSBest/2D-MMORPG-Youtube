using System;

namespace ReliableUdp.NetworkStatistic
{
    public class NetworkStatisticManagement
	{
		public FlowManagement FlowManagement { get; private set; }

        private int rtt;
		private int avgRtt;
		private int rttCount;
		private int goodRttCount;

        public int TimeSinceLastPacket { get; private set; }

        public int Ping { get; private set; }

        public double ResendDelay
		{
			get { return this.avgRtt; }
		}

		private const int RTT_RESET_DELAY = 1000;
		private int rttResetTimer;

        public NetworkStatisticManagement()
		{
			this.FlowManagement = new FlowManagement();

			// NOTE: we start with an avgRtt because we don't want to have a resent delay of 0
			this.avgRtt = 27;
			this.rtt = 0;
		}

		public void ResetTimeSinceLastPacket()
		{
			this.TimeSinceLastPacket = 0;
		}

		public void UpdateRoundTripTime(int roundTripTime)
		{
			this.rtt += roundTripTime;
			this.rttCount++;
			this.avgRtt = this.rtt / this.rttCount;
			if (this.avgRtt < this.FlowManagement.GetStartRtt(this.FlowManagement.CurrentFlowMode - 1))
			{
				if (this.FlowManagement.CurrentFlowMode <= 0)
				{
					return;
				}

				this.goodRttCount++;
				if (this.goodRttCount > FlowManagement.FLOW_INCREASE_THRESHOLD)
				{
					this.goodRttCount = 0;
					this.FlowManagement.CurrentFlowMode--;

#if UDP_DEBUGGING
					System.Diagnostics.Debug.WriteLine($"Increased flow speed, RTT {this.avgRtt}, PPS {this.FlowManagement.GetPacketsPerSecond(this.FlowManagement.CurrentFlowMode)}");
#endif
				}
			}
			else if (this.avgRtt > this.FlowManagement.GetStartRtt(this.FlowManagement.CurrentFlowMode))
			{
				this.goodRttCount = 0;
				if (this.FlowManagement.CurrentFlowMode < this.FlowManagement.GetMaxFlowMode())
				{
					this.FlowManagement.CurrentFlowMode++;
#if UDP_DEBUGGING
        			System.Diagnostics.Debug.WriteLine($"Decreased flow speed, RTT {this.avgRtt}, PPS {this.FlowManagement.GetPacketsPerSecond(this.FlowManagement.CurrentFlowMode)}");
#endif
				}
			}

            if (this.avgRtt <= 0)
                this.avgRtt = 1;
		}

		public void Update(UdpPeer peer, int deltaTime, Action<UdpPeer, int> connectionLatencyUpdated)
		{
			this.FlowManagement.UpdateFlowTimer(deltaTime);
			this.TimeSinceLastPacket += deltaTime;

			this.rttResetTimer += deltaTime;
			if (this.rttResetTimer >= RTT_RESET_DELAY)
			{
				this.rttResetTimer = 0;
				this.rtt = this.avgRtt;
				this.Ping = this.avgRtt;
				this.rttCount = 1;
				connectionLatencyUpdated(peer, this.Ping);
			}
		}
	}
}
