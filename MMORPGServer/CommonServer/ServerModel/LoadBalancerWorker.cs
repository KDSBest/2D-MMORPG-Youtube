using CommonServer.ServerModel.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonServer.ServerModel
{
	public abstract class LoadBalancerWorker<T> : Server where T : class, INameable
	{
		public string RedisKeyNamePrefix { get; private set; }
		public int LoadBalanceUpdateDelay { get; private set; }

		public List<string> Jobs = new List<string>();

		private int currentLoadBalanceUpdateDelay = 0;

		private RedisServerHeartbeatRepository serverHeartbeatRepo;
		private RedisServerPerformanceRepository serverPerformanceRepo;
		private RedisServerWorkerListRepository serverWorkerListRepo;
		private RedisServerWorkerJobRepository<T> serverWorkerJobRepo;
		private LoadBalancerJobCache<T> loadBalancerJobCache;

		public LoadBalancerWorker(string redisKeyNamePrefix, int loadBalanceUpdateDelay = 2000, int updateDelay = 100, int maxPrimaryTrustDelay = 2000) : base(Guid.NewGuid(), updateDelay)
		{
			LoadBalanceUpdateDelay = loadBalanceUpdateDelay;

			RedisKeyNamePrefix = redisKeyNamePrefix;
			serverHeartbeatRepo = new RedisServerHeartbeatRepository(redisKeyNamePrefix, maxPrimaryTrustDelay);
			serverPerformanceRepo = new RedisServerPerformanceRepository(redisKeyNamePrefix);
			serverWorkerListRepo = new RedisServerWorkerListRepository(redisKeyNamePrefix);
			serverWorkerJobRepo = new RedisServerWorkerJobRepository<T>(redisKeyNamePrefix);
			loadBalancerJobCache = new LoadBalancerJobCache<T>(serverWorkerJobRepo);
		}

		protected abstract Task HandleLoad(T jobContext);

		protected abstract void PrepareUpdate();

		protected override async Task Update()
		{
			currentLoadBalanceUpdateDelay -= this.UpdateDelay;
			serverHeartbeatRepo.UpdateHeartbeat(this.Id);

			Console.WriteLine($"Prepare Jobs");
			PrepareUpdate();
			List<Task> jobs = new List<Task>();
			Console.WriteLine($"Handle {this.Jobs.Count} Jobs");
			foreach(var job in this.Jobs)
			{
				var jobScoped = job;
				jobs.Add(Task.Run(async () =>
				{
					var jobContext = loadBalancerJobCache.Get(jobScoped);
					await HandleLoad(jobContext);
				}));
			}
			await Task.WhenAll(jobs);

			if(currentLoadBalanceUpdateDelay <= 0)
			{
				serverPerformanceRepo.SetPerformance(this.Id, this.UpdateDuration);
				Console.WriteLine($"Update Loop took {this.UpdateDuration} ms");
				Jobs = serverWorkerJobRepo.GetJobs(this.Id);

				// when we have no load two things can be the case
				// a) we are a new worker
				// b) Load Distribution removed all our work, because we are not trustworthy
				if (Jobs.Count == 0)
				{
					serverWorkerListRepo.AddWorker(this.Id);
				}
			}
		}
	}
}
