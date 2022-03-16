using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonServer.PrimarySecondary
{
	public class LoadBalancerDistribution<T> where T : class
	{
		public string RedisKeyNamePrefix { get; }

		public Dictionary<T, string> DistributedLoad = new Dictionary<T, string>();
		public List<T> Load = new List<T>();

		public LoadBalancerDistribution(string redisKeyNamePrefix)
		{
			RedisKeyNamePrefix = redisKeyNamePrefix;
		}

		internal void AddLoadEntry(T enemyLoadEntry) 
		{
			Load.Add(enemyLoadEntry);
			DistributedLoad.Add(enemyLoadEntry, string.Empty);
		}
	}

	public class LoadBalancerServer<T> : PrimarySecondaryServer where T : class
	{
		private LoadBalancerDistribution<T> loadBalancerDistribution;

		public LoadBalancerServer(string redisKeyNamePrefix) : base(redisKeyNamePrefix)
		{
			loadBalancerDistribution = new LoadBalancerDistribution<T>(redisKeyNamePrefix);
			OnPrimaryUpdate = BalanceLoad;
			OnPrimaryStart = InitializeCache;
		}

		private void InitializeCache()
		{
		}

		public void BalanceLoad()
		{
		}

		public void AddLoadEntry(T enemyLoadEntry)
		{
			loadBalancerDistribution.AddLoadEntry(enemyLoadEntry);
		}
	}

	public class LoadBalancerWorker<T> where T : class
	{
	}
}
