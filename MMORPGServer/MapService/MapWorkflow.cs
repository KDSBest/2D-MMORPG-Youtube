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
using Common.Protocol.Map.Interfaces;
using Common.GameDesign;
using System.Linq;
using CommonServer.CosmosDb;

namespace MapService
{
	public class MapWorkflow : IJwtWorkflow
	{
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }

		private PlayerWorldState worldState = new PlayerWorldState();
		private const int maxPackageSize = 1400;
		private string playerName = string.Empty;
		private IPubSub pubsubLocal;
		private UdpPeer peer;
		private MapPartitionManagement mapPartitionManagement = new MapPartitionManagement();
		private PlayerStateMessage lastAcceptedPlayerStateMessage = null;
		private MapData mapData;
		private MapRepository repo = new MapRepository();
		private long lastServerTimeSave;

		public MapWorkflow()
		{
			pubsubLocal = DI.Instance.Resolve<IPubSub>();
			mapData = DI.Instance.Resolve<MapData>();
		}

		public async Task OnStartAsync(UdpPeer peer)
		{
			this.peer = peer;

			if (lastAcceptedPlayerStateMessage != null)
			{
				ForcePlayerState(lastAcceptedPlayerStateMessage);
			}
		}

		public async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
			await HandleSave(DateTime.UtcNow.Ticks, true);

			pubsubLocal.Unsubscribe<RemoveStateMessage>(playerName);
			pubsubLocal.Unsubscribe<PlayerWorldEvent<PlayerStateMessage>>(playerName);
			pubsubLocal.Unsubscribe<PlayerWorldEvent<PropStateMessage>>(playerName);
			RedisPubSub.Publish<RemoveStateMessage>(RedisConfiguration.MapChannelRemoveStatePrefix + MapConfiguration.MapName, new RemoveStateMessage()
			{
				Name = playerName,
				ServerTime = DateTime.UtcNow.Ticks,
				Partition = new Vector2Int(0, 0)
			});
		}

		public async Task OnLatencyUpdateAsync(int latency)
		{
		}

		public async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			long serverTime = DateTime.UtcNow.Ticks;

			var playerStateMessage = new PlayerStateMessage();
			if(playerStateMessage.Read(reader))
			{
				// newer than our time?
				if (playerStateMessage.ServerTime > serverTime)
					return;

				// too old (2 seconds)
				var stateMessageServerTimeDiff = serverTime - playerStateMessage.ServerTime;
				if (stateMessageServerTimeDiff > MapConfiguration.MaxPlayerStateTime)
					return;

				if (lastAcceptedPlayerStateMessage != null)
				{
					if (lastAcceptedPlayerStateMessage.ServerTime > playerStateMessage.ServerTime)
						return;

					if (lastAcceptedPlayerStateMessage.ServerTime == playerStateMessage.ServerTime)
						return;

					float diffPlayerStateTime = (float)(playerStateMessage.ServerTime - lastAcceptedPlayerStateMessage.ServerTime) / (float)TimeSpan.TicksPerSecond;

					if (diffPlayerStateTime == 0)
						return;

					float diffPlayerPosition = (playerStateMessage.Position - lastAcceptedPlayerStateMessage.Position).LengthSquared();
					float playerSpeedPerSecond = diffPlayerPosition / diffPlayerStateTime;

					if (playerSpeedPerSecond > MapConfiguration.MaxPlayerSpeedSquared + MapConfiguration.MaxPlayerSpeedEpsilon)
					{
						lastAcceptedPlayerStateMessage.ServerTime = serverTime;
						ForcePlayerState(lastAcceptedPlayerStateMessage);
						return;
					}
				}

				playerStateMessage.Name = this.playerName;

				var removedPartitions = mapPartitionManagement.UpdatePlayerPartitionRegistrations(playerStateMessage);
				foreach (var removedPartition in removedPartitions)
				{
					RemovePartition(removedPartition);
				}

				lastAcceptedPlayerStateMessage = playerStateMessage;

				// no await, because we don't want to block for save
				HandleSave(serverTime, false);

				RedisPubSub.Publish<PlayerStateMessage>(RedisConfiguration.MapChannelNewPlayerStatePrefix + MapConfiguration.MapName, playerStateMessage);

				var worldPackage = worldState.GetPackage(maxPackageSize);
				if (worldPackage.Length > 0)
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

			var teleportMessage = new TeleportMessage();
			if (teleportMessage.Read(reader))
			{
				var npc = mapData.NPCs.FirstOrDefault(x => x.Name == teleportMessage.Name);

				if (npc == null || !npc.IsTeleporter)
					return;

				if (lastAcceptedPlayerStateMessage == null)
					return;

				lastAcceptedPlayerStateMessage.ServerTime = serverTime;
				lastAcceptedPlayerStateMessage.Position = npc.Position;
				ForcePlayerState(lastAcceptedPlayerStateMessage);

				return;
			}
		}

		private async Task HandleSave(long serverTime, bool force)
		{
			if (force || serverTime - lastServerTimeSave > MapConfiguration.ServerSaveTime)
			{
				lastServerTimeSave = serverTime;
				await repo.SaveAsync(lastAcceptedPlayerStateMessage, this.playerName);
			}
		}

		private void ForcePlayerState(PlayerStateMessage state)
		{
			lastAcceptedPlayerStateMessage = state;
			var forceMessage = state.Clone();
			forceMessage.ForcePosition = true;
			UdpManager.SendMsg(this.peer.ConnectId, forceMessage, ChannelType.Reliable);
		}

		public void OnToken(string token)
		{
			playerName = JwtTokenHelper.GetTokenClaim(token, SecurityConfiguration.CharClaimType);
			pubsubLocal.Subscribe<PlayerWorldOneTimeEvent<SkillCastMessage>>(OnNewSkillEffect, playerName);
			pubsubLocal.Subscribe<PlayerWorldEvent<PropStateMessage>>(OnNewState, playerName);
			pubsubLocal.Subscribe<PlayerWorldEvent<PlayerStateMessage>>(OnNewState, playerName);
			pubsubLocal.Subscribe<RemoveStateMessage>(OnPlayerDisconnected, playerName);

			lastServerTimeSave = DateTime.UtcNow.Ticks;
			lastAcceptedPlayerStateMessage = repo.GetAsync(playerName).Result;
		}

		private void OnNewSkillEffect(PlayerWorldOneTimeEvent<SkillCastMessage> ev)
		{
			if (!mapPartitionManagement.IsRegistered(ev.Partition))
				return;

			// no need to send player stats to client
			ev.State.CasterStats = null;

			UdpManager.SendMsg(this.peer.ConnectId, ev.State, ChannelType.Reliable);
		}

		private void OnPlayerDisconnected(RemoveStateMessage msg)
		{
			if(mapPartitionManagement.IsRegistered(msg.Partition))
			{
				UdpManager.SendMsg(this.peer.ConnectId, msg, ChannelType.Reliable);
			}
		}

		private void OnNewState<T>(PlayerWorldEvent<T> ev) where T : IMapStateMessage
		{
			if (!mapPartitionManagement.IsRegistered(ev.NewPartition))
			{
				if (ev.OldPartition != null && mapPartitionManagement.IsRegistered(ev.OldPartition))
				{
					RemoveState(ev.State.Name);
				}
				return;
			}

			worldState.AddState(ev.State.Name, new State(ev.State.Name, ev.State, MapConfiguration.PlayerPriority, ev.NewPartition));
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
