using Common;
using Common.IoC;
using Common.Protocol.Map;
using Common.PublishSubscribe;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonServer.WorldManagement
{
	public class MapPartitionManagement
	{
		private List<Vector2Int> registeredPartitions = new List<Vector2Int>(18);
		private Vector2Int lastPartition = null;

		public List<Vector2Int> GetRegisteredPartitions()
		{
			return registeredPartitions.ToList();
		}

		public List<Vector2Int> UpdatePlayerPartitionRegistrations(PlayerStateMessage playerStateMessage)
		{
			var newPartition = new Vector2Int(playerStateMessage.Position);

			if (newPartition == lastPartition)
				return new List<Vector2Int>();

			RegisterPartitionsAround(newPartition);
			lastPartition = newPartition;
			return UnregisterTooFarPartitions(newPartition, playerStateMessage.ServerTime);
		}

		private List<Vector2Int> UnregisterTooFarPartitions(Vector2Int newPartition, long serverTime)
		{
			List<Vector2Int> removedPartitions = new List<Vector2Int>();

			var cloned = registeredPartitions.ToList();
			foreach(var partition in cloned)
			{
				int xDist = Math.Abs(partition.X - newPartition.X);
				int yDist = Math.Abs(partition.Y - newPartition.Y);
				int dist = Math.Max(xDist, yDist) / MapConfiguration.MapAreaSize;
				if (dist > MapConfiguration.UnregistrationBorder)
				{
					registeredPartitions.Remove(partition);
					removedPartitions.Add(partition);
				}
			}

			return removedPartitions;
		}

		private void RegisterPartitionsAround(Vector2Int lastPartition)
		{
			int distance = MapConfiguration.MapAreaSize * MapConfiguration.RegistrationBorder;
			for (int y = lastPartition.Y - distance; y <= lastPartition.Y + distance; y += MapConfiguration.MapAreaSize)
			{
				for (int x = lastPartition.X - distance; x <= lastPartition.X + distance; x += MapConfiguration.MapAreaSize)
				{
					RegisterPartition(new Vector2Int(x, y));
				}
			}

			this.lastPartition = lastPartition;
		}

		private void RegisterPartition(Vector2Int partition)
		{
			if (!IsRegistered(partition))
				registeredPartitions.Add(partition);
		}

		public bool IsRegistered(Vector2Int partition)
		{
			return registeredPartitions.Contains(partition);
		}
	}
}
