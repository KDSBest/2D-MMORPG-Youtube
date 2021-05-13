using Common.Crypto;
using Common.Extensions;
using Common.Protocol.Map;
using Common.PublishSubscribe;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Common.Client.Workflow
{
	public class MapWorkflow : ICryptoWorkflow
	{
		public CryptoProvider Crypto { get; set; }
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }

		private long UtcDiff = 0;
		private const int ResendTimeSyncMs = 1000;
		private const int TimeSyncRuns = 3;

		public IPubSub PubSub { get; set; }

		public async Task OnStartAsync(UdpPeer peer)
		{
			_ = Task.Run(async () =>
			  {
				  for (int run = 0; run < TimeSyncRuns; run++)
				  {
					  var timeSyncMsg = new TimeSyncMessage()
					  {
						  MyTime = DateTime.UtcNow.Ticks
					  };
					  UdpManager.SendMsg(timeSyncMsg, ChannelType.Reliable);

					  await Task.Delay(ResendTimeSyncMs);
				  }
			  });
		}

		public async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
		}

		public async Task OnLatencyUpdateAsync(int latency)
		{
		}

		public async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			var playerMsg = new PlayerStateMessage();
			if (playerMsg.Read(reader))
			{
				PubSub.Publish(playerMsg);
				return;
			}

			var timeSyncMsg = new TimeSyncMessage();
			if (timeSyncMsg.Read(reader))
			{
				HandleTimeSyncMessage(timeSyncMsg);
				PubSub.Publish(timeSyncMsg);
				return;
			}
		}

		private void HandleTimeSyncMessage(TimeSyncMessage timeSyncMsg)
		{
			long now = DateTime.UtcNow.Ticks;
			long delay = (now - timeSyncMsg.MyTime) / 2;
			long ourTimeAtServerRecv = now - delay;

			if (UtcDiff == 0)
			{
				UtcDiff = timeSyncMsg.ServerTime - ourTimeAtServerRecv;
			}
			else
			{
				UtcDiff += timeSyncMsg.ServerTime - ourTimeAtServerRecv;
				UtcDiff /= 2;
			}
		}

		public long GetServerTime()
		{
			return DateTime.UtcNow.Ticks + UtcDiff;
		}

		public async Task SendStateAsync(Vector2 position, int animation, bool isLookingRight)
		{
			var state = new PlayerStateMessage();
			state.Position = position;
			state.Animation = animation;
			state.IsLookingRight = isLookingRight;
			state.ServerTime = GetServerTime();
			UdpManager.SendMsg(state, ChannelType.UnreliableOrdered);
		}
	}
}
