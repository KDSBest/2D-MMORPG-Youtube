using Common;
using Common.IoC;
using Common.Protocol.Map;
using Common.Protocol.Map.Interfaces;
using Common.PublishSubscribe;
using CommonServer.Configuration;
using CommonServer.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;

namespace MapService.WorldManagement
{

	public class PlayerWorldManagement : IPlayerWorldManagement
	{
		private ConcurrentDictionary<string, IMapStateMessage> LastState = new ConcurrentDictionary<string, IMapStateMessage>();
		private ConcurrentDictionary<string, Vector2Int> LastStatePartition = new ConcurrentDictionary<string, Vector2Int>();
		private IPubSub pubsubLocal;

		public void Initialize()
		{
			pubsubLocal = DI.Instance.Resolve<IPubSub>();
			RedisPubSub.Subscribe<SkillCastMessage>(RedisConfiguration.MapChannelSkillCastPrefix + MapConfiguration.MapName, OnOneTimeEvent);
			RedisPubSub.Subscribe<PropStateMessage>(RedisConfiguration.MapChannelNewPropStatePrefix + MapConfiguration.MapName, OnStateChange);
			RedisPubSub.Subscribe<PlayerStateMessage>(RedisConfiguration.MapChannelNewPlayerStatePrefix + MapConfiguration.MapName, OnStateChange);
			RedisPubSub.Subscribe<RemoveStateMessage>(RedisConfiguration.MapChannelRemoveStatePrefix + MapConfiguration.MapName, OnDisconnectedPlayer);
		}

		private void OnDisconnectedPlayer(RedisChannel channel, RemoveStateMessage msg)
		{
			LastState.TryRemove(msg.Name, out var x);
			if(LastStatePartition.TryRemove(msg.Name, out var lastPlayerPartition))
			{
				msg.Partition = lastPlayerPartition;
				pubsubLocal.Publish(msg);
			}
		}

		private void OnOneTimeEvent<T>(RedisChannel channel, T msg) where T : IPartitionMessage
		{
			var partition = new Vector2Int(msg);
			pubsubLocal.Publish(new PlayerWorldOneTimeEvent<T>(msg, partition));
		}

		private void OnStateChange<T>(RedisChannel channel, T msg) where T : IMapStateMessage<T>
		{
			Vector2Int oldPartition = null;
			Vector2Int newPartition = new Vector2Int(msg);
			if (LastStatePartition.ContainsKey(msg.Name))
			{
				oldPartition = LastStatePartition[msg.Name];
			}

			// Skip unchanged state
			if (oldPartition == newPartition)
			{
				T lastMsg = (T)LastState[msg.Name];

				// state is skipped if:
				// - Position is only changed by small amount
				// - Is Looking Right is unchanged
				// - Animation is unchanged
				// - Last Message isn't older than GameConfiguration.AcceptUnchangedPlayerStateAfterSeconds seconds
				if (lastMsg.ServerTime >= DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(GameConfiguration.AcceptUnchangedPlayerStateAfterSeconds)).Ticks
					&& lastMsg.HasNoVisibleDifference(msg))
					return;
			}

			LastState.AddOrUpdate(msg.Name, msg, (n, s) => msg);
			LastStatePartition.AddOrUpdate(msg.Name, newPartition, (n, s) => newPartition);

			pubsubLocal.Publish(new PlayerWorldEvent<T>(msg, newPartition, oldPartition));
		}
	}
}
