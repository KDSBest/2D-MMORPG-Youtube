using Common.IoC;
using Common.Protocol.Map;
using Common.PublishSubscribe;
using CommonServer.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MapService.WorldManagement
{

	public class PlayerWorldManagement : IPlayerWorldManagement
	{
		private ConcurrentDictionary<string, PlayerStateMessage> LastPlayerPosition = new ConcurrentDictionary<string, PlayerStateMessage>();
		private ConcurrentDictionary<string, MapPartition> LastPlayerPartition = new ConcurrentDictionary<string, MapPartition>();
		private IPubSub pubsub;

		public void Initialize()
		{
			pubsub = DI.Instance.Resolve<IPubSub>();
			RedisPubSub.Subscribe<PlayerStateMessage>(MapConfiguration.MapName, OnNewPlayerState);
		}

		public void OnDisconnectedPlayer(string name)
		{
			LastPlayerPosition.TryRemove(name, out var x);
			LastPlayerPartition.TryRemove(name, out var y);
		}

		private void OnNewPlayerState(RedisChannel channel, PlayerStateMessage msg)
		{
			MapPartition oldPartition = null;
			MapPartition newPartition = new MapPartition(msg);
			if(LastPlayerPartition.ContainsKey(msg.Name))
			{
				oldPartition = LastPlayerPartition[msg.Name];
			}

			// Skip unchanged state
			if(oldPartition == newPartition)
			{
				var lastMsg = LastPlayerPosition[msg.Name];
				if (Math.Abs(lastMsg.Position.X - msg.Position.X) < MapConfiguration.SmallDistance
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
