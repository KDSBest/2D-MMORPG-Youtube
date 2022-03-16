using CommonServer.Redis;
using System;

namespace CommonServer.ServerModel.Repos
{
	public class RedisServerPerformanceRepository
	{
		public string RedisKeyNamePrefix { get; private set; }
		public string RedisKeyPerformance { get; private set; }

		public RedisServerPerformanceRepository(string redisKeyNamePrefix)
		{
			this.SetRedisKeyNamePrefix(redisKeyNamePrefix);
		}

		private void SetRedisKeyNamePrefix(string redisKeyNamePrefix)
		{
			RedisKeyNamePrefix = redisKeyNamePrefix;
			RedisKeyPerformance = $"{RedisKeyNamePrefix}_Performance_";
		}

		public string GetKey(Guid id)
		{
			return $"{RedisKeyPerformance}{id}";
		}

		public void SetPerformance(Guid id, long performance)
		{
			RedisKV.Set(GetKey(id), performance.ToString());
		}

		public long GetPerformance(Guid id)
		{
			long performance = -1;
			if (!long.TryParse(RedisKV.Get(GetKey(id)), out performance))
				performance = -1;

			return performance;
		}
	}
}
