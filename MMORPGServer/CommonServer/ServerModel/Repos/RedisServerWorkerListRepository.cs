using CommonServer.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonServer.ServerModel.Repos
{
	public class RedisServerWorkerListRepository
	{
		public string RedisKeyNamePrefix { get; private set; }
		public string RedisKeyWorker { get; private set; }

		public RedisServerWorkerListRepository(string redisKeyNamePrefix)
		{
			this.SetRedisKeyNamePrefix(redisKeyNamePrefix);
		}

		private void SetRedisKeyNamePrefix(string redisKeyNamePrefix)
		{
			RedisKeyNamePrefix = redisKeyNamePrefix;
			RedisKeyWorker = $"{RedisKeyNamePrefix}_Worker";
		}

		public void AddWorker(Guid id)
		{
			RedisList.AddUnique(RedisKeyWorker, id.ToString());
		}

		public List<Guid> GetAllWorker()
		{
			return RedisList.Get(RedisKeyWorker).ConvertAll(x => new Guid(x)).ToList();
		}
	}
}
