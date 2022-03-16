using CommonServer.Redis;
using System;

namespace CommonServer.ServerModel.Repos
{
	public class RedisServerPrimaryRepository
	{
		public string RedisKeyNamePrefix { get; private set; }
		public string RedisKeyPrimary { get; private set; }

		public RedisServerPrimaryRepository(string redisKeyNamePrefix)
		{
			this.SetRedisKeyNamePrefix(redisKeyNamePrefix);
		}

		private void SetRedisKeyNamePrefix(string redisKeyNamePrefix)
		{
			RedisKeyNamePrefix = redisKeyNamePrefix;
			RedisKeyPrimary = $"{RedisKeyNamePrefix}_Primary";
		}

		public string GetPrimary()
		{
			return RedisKV.Get(RedisKeyPrimary);
		}

		public void TrySetPrimary(Guid id)
		{
			RedisKV.SetIfNotExists(RedisKeyPrimary, id.ToString());
		}
		public void ForcefullyRemovePrimary()
		{
			RedisKV.Remove(this.RedisKeyPrimary);
		}
	}
}
