using CommonServer.ServerModel.Repos;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CommonServer.ServerModel
{
	public class LoadBalancerJobCache<T> where T : INameable
	{
		public RedisServerWorkerJobRepository<T> ServerWorkerJobRepo { get; }
		private ConcurrentDictionary<string, T> cache = new ConcurrentDictionary<string, T>();

		public LoadBalancerJobCache(RedisServerWorkerJobRepository<T> serverWorkerJobRepo)
		{
			ServerWorkerJobRepo = serverWorkerJobRepo;
		}

		public T Get(string name)
		{
			if(!cache.ContainsKey(name) || cache[name] == null)
			{
				var job = ServerWorkerJobRepo.GetJob(name);
				cache.AddOrUpdate(name, job, (key, val) => job);
			}

			return cache[name];
		}
	}
}
