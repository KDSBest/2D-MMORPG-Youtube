using System;
using System.Threading.Tasks;
using CommonServer.Workflow;
using ReliableUdp.Enums;
using CommonServer.Configuration;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Utility;
using CommonServer;
using Common.Protocol.Map;
using Common.Extensions;
using CommonServer.Redis;
using MapService.WorldManagement;
using Common.IoC;
using Common.PublishSubscribe;

namespace MapService
{
	public class MapWorkflow : IJwtWorkflow
	{
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }

		private PlayerWorldState worldState = new PlayerWorldState();
		private const int maxPackageSize = 1400;
		private string name = string.Empty;
		private IPubSub pubsub;
		private UdpPeer peer;
		private MapPartitionManagement mapPartitionManagement = new MapPartitionManagement();

		public MapWorkflow()
		{
			pubsub = DI.Instance.Resolve<IPubSub>();
		}

		public async Task OnStartAsync(UdpPeer peer)
		{
			this.peer = peer;
		}

		public async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
			pubsub.Unsubscribe<RemoveStateMessage>(name);
			pubsub.Unsubscribe<PlayerWorldEvent<PlayerStateMessage>>(name);
			RedisPubSub.Publish<RemoveStateMessage>(MapConfiguration.MapName, new RemoveStateMessage()
			{
				Name = name,
				ServerTime = DateTime.UtcNow.Ticks
			});
		}

		public async Task OnLatencyUpdateAsync(int latency)
		{
		}

		public async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			var playerStateMessage = new PlayerStateMessage();
			if(playerStateMessage.Read(reader))
			{
				long serverTime = DateTime.UtcNow.Ticks;

				// newer than our time?
				if (playerStateMessage.ServerTime > serverTime)
					return;

				// too old (2 seconds)
				if (serverTime - playerStateMessage.ServerTime > MapConfiguration.MaxPlayerStateTime)
					return;

				playerStateMessage.Name = this.name;

				mapPartitionManagement.UpdatePlayerPartitionRegistrations(playerStateMessage);

				RedisPubSub.Publish<PlayerStateMessage>(MapConfiguration.MapName, playerStateMessage);

				var worldPackage = worldState.GetPackage(maxPackageSize);
				if(worldPackage.Length > 0)
				{
					peer.Send(worldPackage, ChannelType.Unreliable);
				}

				return;
			}

			var timeSync = new TimeSyncMessage();
			if (timeSync.Read(reader))
			{
				timeSync.ServerTime = DateTime.UtcNow.Ticks;
				UdpManager.SendMsg(this.peer.ConnectId, timeSync, ChannelType.Reliable);
				return;
			}
		}

		public void OnToken(string token)
		{
			name = JwtTokenHelper.GetTokenClaim(token, SecurityConfiguration.CharClaimType);
			pubsub.Subscribe<PlayerWorldEvent<PlayerStateMessage>>(OnNewPlayerState, name);
			pubsub.Subscribe<RemoveStateMessage>(OnPlayerDisconnected, name);
		}

		private void OnPlayerDisconnected(RemoveStateMessage msg)
		{
			UdpManager.SendMsg(this.peer.ConnectId, msg, ChannelType.Reliable);
		}

		private void OnNewPlayerState(PlayerWorldEvent<PlayerStateMessage> pwe)
		{
			if(!mapPartitionManagement.IsRegistered(pwe.NewPartition))
			{
				if(pwe.OldPartition != null && mapPartitionManagement.IsRegistered(pwe.OldPartition))
				{
					worldState.RemoveState(pwe.State.Name);

					UdpManager.SendMsg(this.peer.ConnectId, new RemoveStateMessage()
					{
						Name = pwe.State.Name,
						ServerTime = DateTime.UtcNow.Ticks
					}, ChannelType.Reliable);
				}
				return;
			}

			worldState.AddState(pwe.State.Name, new State(pwe.State, MapConfiguration.PlayerPriority));
		}
	}

}
