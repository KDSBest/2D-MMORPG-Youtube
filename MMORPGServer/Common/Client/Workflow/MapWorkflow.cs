using Common.Extensions;
using Common.Protocol.Map;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Common.Client.Workflow
{
	public class MapWorkflow : BaseJwtWorkflow, IWorkflow
	{
		private long UtcDiff = 0;
		private const int ResendTimeSyncMs = 1000;
		private const int TimeSyncRuns = 3;

		public override async Task OnStartAsync(UdpPeer peer)
		{
			await base.OnStartAsync(peer);

			_ = Task.Run(async () =>
			  {
				  while(!HasServerTokenAccepted)
				  {
					  await Task.Delay(ResendTimeSyncMs);
				  }

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

		public override async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
		}

		public override async Task OnLatencyUpdateAsync(int latency)
		{
		}

		public override async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			int lastPosition = -1;
			while (!reader.EndOfData && reader.AvailableBytes > 0 && reader.Position != lastPosition)
			{
				var playerMsg = new PlayerStateMessage();
				if (playerMsg.Read(reader))
				{
					PubSub.Publish(playerMsg);
					continue;
				}

				var propMsg = new PropStateMessage();
				if(propMsg.Read(reader))
				{
					PubSub.Publish(propMsg);
					continue;
				}

				var removeState = new RemoveStateMessage();
				if (removeState.Read(reader))
				{
					PubSub.Publish(removeState);
					continue;
				}

				var timeSyncMsg = new TimeSyncMessage();
				if (timeSyncMsg.Read(reader))
				{
					HandleTimeSyncMessage(timeSyncMsg);
					PubSub.Publish(timeSyncMsg);
					continue;
				}

				await base.OnReceiveAsync(reader, channel);

				lastPosition = reader.Position;
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
			if (!HasServerTokenAccepted)
				return;

			var state = new PlayerStateMessage();
			state.Position = position;
			state.Animation = animation;
			state.IsLookingRight = isLookingRight;
			state.ServerTime = GetServerTime();
			UdpManager.SendMsg(state, ChannelType.UnreliableOrdered);
		}
	}
}
