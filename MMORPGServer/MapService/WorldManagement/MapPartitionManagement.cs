using Common.Protocol.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapService.WorldManagement
{
	public class MapPartitionManagement
	{
		private List<MapPartition> registeredPartitions = new List<MapPartition>(18);
		private MapPartition lastPartition = null;

		public void UpdatePartitions(PlayerStateMessage playerStateMessage)
		{
			var newPartition = new MapPartition(playerStateMessage.Position);

			if (newPartition == lastPartition)
				return;

			RegisterPartitionsAround(newPartition);
			UnregisterTooFarPartitions(newPartition);
			lastPartition = newPartition;
		}

		private void UnregisterTooFarPartitions(MapPartition newPartition)
		{
			var cloned = registeredPartitions.ToList();
			foreach(var partition in cloned)
			{
				int xDist = Math.Abs(partition.Vector.X - newPartition.Vector.X);
				int yDist = Math.Abs(partition.Vector.Y - newPartition.Vector.Y);
				int dist = Math.Max(xDist, yDist) / MapConfiguration.MapAreaSize;
				if (dist > MapConfiguration.UnregistrationBorder)
				{
					registeredPartitions.Remove(partition);
				}
			}
		}

		private void RegisterPartitionsAround(MapPartition lastPartition)
		{
			int distance = MapConfiguration.MapAreaSize * MapConfiguration.RegistrationBorder;
			for (int y = lastPartition.Vector.Y - distance; y <= lastPartition.Vector.Y + distance; y += MapConfiguration.MapAreaSize)
			{
				for (int x = lastPartition.Vector.X - distance; x <= lastPartition.Vector.X + distance; x += MapConfiguration.MapAreaSize)
				{
					RegisterPartition(new MapPartition(x, y));
				}
			}

			this.lastPartition = lastPartition;
		}

		private void RegisterPartition(MapPartition partition)
		{
			if (!IsRegistered(partition))
				registeredPartitions.Add(partition);
		}

		public bool IsRegistered(MapPartition partition)
		{
			return registeredPartitions.Contains(partition);
		}
	}
}
