using CommonServer.Redis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonServer.ServerModel.Repos
{
	public class RedisServerWorkerJobRepository<T> where T : INameable
	{
		public string RedisKeyNamePrefix { get; private set; }
		public string RedisKeyWorker { get; private set; }
		public string RedisKeyLoad { get; private set; }

		public RedisServerWorkerJobRepository(string redisKeyNamePrefix)
		{
			this.SetRedisKeyNamePrefix(redisKeyNamePrefix);
		}

		private void SetRedisKeyNamePrefix(string redisKeyNamePrefix)
		{
			RedisKeyNamePrefix = redisKeyNamePrefix;
			RedisKeyWorker = $"{RedisKeyNamePrefix}_Worker_";
			RedisKeyLoad = $"{RedisKeyNamePrefix}_Load_";
		}

		public string GetJobKey(string name)
		{
			return $"{RedisKeyLoad}{name}";
		}

		public string GetWorkerKey(Guid id)
		{
			return $"{RedisKeyWorker}{id}";
		}

		public void AddJob(T job)
		{
			RedisKV.Set(GetJobKey(job.Name), JsonConvert.SerializeObject(job));
		}

		public void UnAssignWorkerToJob(Guid workerId, string name)
		{
			RedisList.Remove(GetWorkerKey(workerId), name);
		}

		public void UnAssignWorkerAllJobs(Guid workerId)
		{
			RedisList.Clear(GetWorkerKey(workerId));
		}

		public void AssignWorkerToJob(Guid workerId, string name)
		{
			RedisList.AddUnique(GetWorkerKey(workerId), name);
		}

		public List<string> GetJobs(Guid workerId)
		{
			return RedisList.Get(GetWorkerKey(workerId));
		}

		public T GetJob(string name)
		{
			string redisObj = RedisKV.Get(GetJobKey(name));
			return JsonConvert.DeserializeObject<T>(redisObj);
		}
	}
}
