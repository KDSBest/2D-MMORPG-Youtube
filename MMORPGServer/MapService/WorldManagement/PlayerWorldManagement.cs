using Common;
using Common.IoC;
using Common.Protocol.Map;
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
		private ConcurrentDictionary<string, PlayerStateMessage> LastPlayerPosition = new ConcurrentDictionary<string, PlayerStateMessage>();
		private ConcurrentDictionary<string, Vector2Int> LastPlayerPartition = new ConcurrentDictionary<string, Vector2Int>();
		private IPubSub pubsub;

		public void Initialize()
		{
			pubsub = DI.Instance.Resolve<IPubSub>();
			RedisPubSub.Subscribe<PlayerStateMessage>(RedisConfiguration.MapChannelNewStatePrefix + MapConfiguration.MapName, OnNewPlayerState);
			RedisPubSub.Subscribe<RemoveStateMessage>(RedisConfiguration.MapChannelRemoveStatePrefix + MapConfiguration.MapName, OnDisconnectedPlayer);
		}

		private void OnDisconnectedPlayer(RedisChannel channel, RemoveStateMessage msg)
		{
			LastPlayerPosition.TryRemove(msg.Name, out var x);
			if(LastPlayerPartition.TryRemove(msg.Name, out var lastPlayerPartition))
			{
				msg.Partition = lastPlayerPartition;
				pubsub.Publish(msg);
			}
		}

		private void OnNewPlayerState(RedisChannel channel, PlayerStateMessage msg)
		{
			Vector2Int oldPartition = null;
			Vector2Int newPartition = new Vector2Int(msg);
			if(LastPlayerPartition.ContainsKey(msg.Name))
			{
				oldPartition = LastPlayerPartition[msg.Name];
			}

			// Skip unchanged state
			if(oldPartition == newPartition)
			{
				var lastMsg = LastPlayerPosition[msg.Name];

				// state is skipped if:
				// - Position is only changed by small amount
				// - Is Looking Right is unchanged
				// - Animation is unchanged
				// - Last Message isn't older than GameConfiguration.AcceptUnchangedPlayerStateAfterSeconds seconds
				if (lastMsg.ServerTime >= DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(GameConfiguration.AcceptUnchangedPlayerStateAfterSeconds)).Ticks
					&& Math.Abs(lastMsg.Position.X - msg.Position.X) < MapConfiguration.SmallDistance
					&& Math.Abs(lastMsg.Position.Y - msg.Position.Y) < MapConfiguration.SmallDistance
					&& lastMsg.IsLookingRight == msg.IsLookingRight
					&& lastMsg.Animation == msg.Animation)
					return;
			}

			LastPlayerPosition.AddOrUpdate(msg.Name, msg, (n, s) => msg);
			LastPlayerPartition.AddOrUpdate(msg.Name, newPartition, (n, s) => newPartition);
			pubsub.Publish(new PlayerWorldEvent<PlayerStateMessage>(msg, newPartition, oldPartition));
		}
	}
}
