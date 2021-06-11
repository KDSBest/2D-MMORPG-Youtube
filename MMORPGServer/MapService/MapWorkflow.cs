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
using Common;

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
			RedisPubSub.Publish<RemoveStateMessage>(RedisConfiguration.MapChannelRemoveStatePrefix + MapConfiguration.MapName, new RemoveStateMessage()
			{
				Name = name,
				ServerTime = DateTime.UtcNow.Ticks,
				Partition = new Vector2Int(0, 0)
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

				var removedPartitions = mapPartitionManagement.UpdatePlayerPartitionRegistrations(playerStateMessage);
				foreach(var removedPartition in removedPartitions)
				{
					RemovePartition(removedPartition);
				}

				RedisPubSub.Publish<PlayerStateMessage>(RedisConfiguration.MapChannelNewStatePrefix + MapConfiguration.MapName, playerStateMessage);

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
			if(mapPartitionManagement.IsRegistered(msg.Partition))
			{
				UdpManager.SendMsg(this.peer.ConnectId, msg, ChannelType.Reliable);
			}
		}

		private void OnNewPlayerState(PlayerWorldEvent<PlayerStateMessage> pwe)
		{
			if(!mapPartitionManagement.IsRegistered(pwe.NewPartition))
			{
				if(pwe.OldPartition != null && mapPartitionManagement.IsRegistered(pwe.OldPartition))
				{
					RemoveState(pwe.State.Name);
				}
				return;
			}

			worldState.AddState(pwe.State.Name, new State(pwe.State.Name, pwe.State, MapConfiguration.PlayerPriority, pwe.NewPartition));
		}

		private void RemovePartition(Vector2Int partition)
		{
			var partitionState = worldState.GetState(partition);
			foreach(var state in partitionState)
			{
				RemoveState(state.Id);
			}
		}

		private void RemoveState(string name)
		{
			worldState.RemoveState(name);

			SendRemoveState(name);
		}

		private void SendRemoveState(string name)
		{
			UdpManager.SendMsg(this.peer.ConnectId, new RemoveStateMessage()
			{
				Name = name,
				ServerTime = DateTime.UtcNow.Ticks,
				Partition = new Vector2Int(0, 0)
			}, ChannelType.Reliable);
		}
	}

}
