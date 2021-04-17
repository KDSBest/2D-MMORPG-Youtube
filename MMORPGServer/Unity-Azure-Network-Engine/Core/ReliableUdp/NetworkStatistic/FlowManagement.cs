using System;
using System.Collections.Generic;
using ReliableUdp.Utility;

namespace ReliableUdp.NetworkStatistic
{
    public class FlowManagement
	{
		public const int FLOW_INCREASE_THRESHOLD = 4;
		public const int FLOW_UPDATE_TIME = 1000;
		private readonly List<FlowMode> flowModes;
		private int sendedPacketsCount;
		private int flowTimer;
		public int CurrentFlowMode { get; set; }

		public FlowManagement()
		{
			this.flowModes = new List<FlowMode>();
		}
		public void AddFlowMode(int startRtt, int packetsPerSecond)
		{
			var fm = new FlowMode { PacketsPerSecond = packetsPerSecond, StartRtt = startRtt };

			if (this.flowModes.Count > 0 && startRtt < this.flowModes[0].StartRtt)
			{
				this.flowModes.Insert(0, fm);
			}
			else
			{
				this.flowModes.Add(fm);
			}
		}

		public int GetPacketsPerSecond(int flowMode)
		{
			if (flowMode < 0 || this.flowModes.Count == 0)
				return 0;
			return this.flowModes[flowMode].PacketsPerSecond;
		}

		public int GetMaxFlowMode()
		{
			return this.flowModes.Count - 1;
		}

		public int GetStartRtt(int flowMode)
		{
			if (flowMode < 0 || this.flowModes.Count == 0)
				return 0;
			return this.flowModes[flowMode].StartRtt;
		}
		public void UpdateFlowTimer(int deltaTime)
		{
			this.flowTimer += deltaTime;
			if (this.flowTimer >= FLOW_UPDATE_TIME)
			{
#if UDP_DEBUGGING
				System.Diagnostics.Debug.WriteLine($"Reset flow timer, sended packets {this.sendedPacketsCount}");
#endif
				this.sendedPacketsCount = 0;
				this.flowTimer = 0;
			}
		}

		public int GetCurrentMaxSend(int deltaTime)
		{
			int maxSendPacketsCount = GetPacketsPerSecond(this.CurrentFlowMode);

			if (maxSendPacketsCount > 0)
			{
				int availableSendPacketsCount = maxSendPacketsCount - this.sendedPacketsCount;
				return Math.Min(availableSendPacketsCount, (maxSendPacketsCount * deltaTime) / FLOW_UPDATE_TIME);
			}
			else
			{
				return int.MaxValue;
			}
		}

		public void IncreaseSendedPacketCount(int currentSended)
		{
			this.sendedPacketsCount += currentSended;
		}
	}
}
