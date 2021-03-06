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
		private const int ResendTimeSyncMs = 1000;
		private const int TimeSyncRuns = 3;

		public override async Task OnStartAsync(UdpPeer peer)
		{
			await base.OnStartAsync(peer);

			_ = Task.Run(async () =>
			  {
				  while (!HasServerTokenAccepted)
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

				var propMsg = new EnemyStateMessage();
				if (propMsg.Read(reader))
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

				var skillCast = new SkillCastMessage();
				if (skillCast.Read(reader))
				{
					PubSub.Publish(skillCast);
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

			if (ServerTimeTracking.UtcDiff == 0)
			{
				ServerTimeTracking.UtcDiff = timeSyncMsg.ServerTime - ourTimeAtServerRecv;
			}
			else
			{
				ServerTimeTracking.UtcDiff += timeSyncMsg.ServerTime - ourTimeAtServerRecv;
				ServerTimeTracking.UtcDiff /= 2;
			}
		}

		public async Task SendTeleportTo(string name)
		{
			if (!HasServerTokenAccepted)
				return;

			var message = new TeleportMessage()
			{
				Name = name
			};
			UdpManager.SendMsg(message, ChannelType.Reliable);
		}

		public async Task SendStateAsync(Vector2 position, int animation, bool isLookingRight)
		{
			if (!HasServerTokenAccepted)
				return;

			var state = new PlayerStateMessage();
			state.Position = position;
			state.Animation = animation;
			state.IsLookingRight = isLookingRight;
			state.ServerTime = ServerTimeTracking.GetServerTime();
			UdpManager.SendMsg(state, ChannelType.UnreliableOrdered);
		}
	}
}
