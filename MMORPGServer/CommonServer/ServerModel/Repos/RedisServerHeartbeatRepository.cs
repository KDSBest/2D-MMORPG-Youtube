using CommonServer.Redis;
using System;

namespace CommonServer.ServerModel.Repos
{
	public class RedisServerHeartbeatRepository
	{
		public string RedisKeyNamePrefix { get; private set; }
		public string RedisKeyHeartbeatPrefix { get; private set; }
		public int MaxPrimaryTrustDelay { get; private set; }

		public RedisServerHeartbeatRepository(string redisKeyNamePrefix, int maxPrimaryTrustDelay = 2000)
		{
			MaxPrimaryTrustDelay = maxPrimaryTrustDelay;
			this.SetRedisKeyNamePrefix(redisKeyNamePrefix);
		}

		private void SetRedisKeyNamePrefix(string redisKeyNamePrefix)
		{
			RedisKeyNamePrefix = redisKeyNamePrefix;
			RedisKeyHeartbeatPrefix = $"{RedisKeyNamePrefix}_Heartbeat_";
		}

		public string GetKey(Guid id)
		{
			return $"{RedisKeyHeartbeatPrefix}{id}";
		}

		public void UpdateHeartbeat(Guid id)
		{
			RedisKV.Set(this.GetKey(id), DateTime.UtcNow.Ticks.ToString());
		}

		public void Remove(Guid id)
		{
			RedisKV.Remove(this.GetKey(id));
		}

		public bool IsHeartbeatOk(Guid id)
		{
			string hbTicksStr = RedisKV.Get(this.GetKey(id));
			if (string.IsNullOrEmpty(hbTicksStr))
			{
				Console.WriteLine($"No Heartbeat for id - {id}");
				return false;
			}

			long ticks;
			if (!long.TryParse(hbTicksStr, out ticks))
			{
				Console.WriteLine($"Heartbeat parsing error for id - {id}");
				return false;
			}

			DateTime utcHbTime = new DateTime(ticks);
			TimeSpan lastHbTime = DateTime.UtcNow - utcHbTime;
			return lastHbTime.TotalMilliseconds <= this.MaxPrimaryTrustDelay;
		}
	}
}
