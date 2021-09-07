using Common.Protocol.Map;
using System.Collections.Generic;

namespace Assets.Scripts.Remoting
{
	public class RemotePlayerRemovedTracking
	{
		private Dictionary<string, long> remotePlayersRemovedTime = new Dictionary<string, long>();

		public long GetPlayerRemovedServerTime(string name)
		{
			if (!remotePlayersRemovedTime.ContainsKey(name))
				return -1;

			return remotePlayersRemovedTime[name];
		}

		public void UpdateRemovedPlayerStateTime(RemoveStateMessage data)
		{
			if (remotePlayersRemovedTime.ContainsKey(data.Name))
			{
				if (remotePlayersRemovedTime[data.Name] < data.ServerTime)
					remotePlayersRemovedTime[data.Name] = data.ServerTime;
				return;
			}

			remotePlayersRemovedTime.Add(data.Name, data.ServerTime);
		}
	}
}
