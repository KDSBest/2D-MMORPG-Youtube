using CommonServer.ServerModel.Repos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonServer.ServerModel
{
	public class LoadBalancerServer<T> : PrimarySecondaryServer where T : class, INameable
	{
		public long RebalanceCost = 50;
		public int MaxRebalancePerUpdate = 100;

		private RedisServerHeartbeatRepository serverHeartbeatRepo;
		private RedisServerPerformanceRepository serverPerformanceRepo;
		private RedisServerWorkerListRepository serverWorkerListRepo;
		private RedisServerWorkerJobRepository<T> serverWorkerJobRepo;

		private Dictionary<string, T> trackedJobs = new Dictionary<string, T>();

		private Dictionary<Guid, List<string>> assignedWorkerJobs = new Dictionary<Guid, List<string>>();
		private Dictionary<string, Guid> jobToWorker = new Dictionary<string, Guid>();
		private List<string> unassignedJobs = new List<string>();

		private List<Guid> trustfulWorker = new List<Guid>();
		private List<Guid> untrustfulWorker = new List<Guid>();
		private Dictionary<Guid, long> trustfulPerformance = new Dictionary<Guid, long> ();
		private long avgJobDuration = 0;
		private long avgWorkerDuration = 0;

		public LoadBalancerServer(string redisKeyNamePrefix, int updateDelay = 1000, int maxPrimaryTrustDelay = 10000) : base(redisKeyNamePrefix, Guid.NewGuid(), updateDelay, maxPrimaryTrustDelay)
		{
			OnPrimaryUpdate = BalanceLoad;

			serverHeartbeatRepo = new RedisServerHeartbeatRepository(redisKeyNamePrefix, this.MaxPrimaryTrustDelay);
			serverPerformanceRepo = new RedisServerPerformanceRepository(redisKeyNamePrefix);
			serverWorkerListRepo = new RedisServerWorkerListRepository(redisKeyNamePrefix);
			serverWorkerJobRepo = new RedisServerWorkerJobRepository<T>(redisKeyNamePrefix);
		}

		public void BalanceLoad()
		{
			this.UpdateWorker();
			this.UnassignUntrustfulWorker();

			if(this.trustfulWorker.Count == 0)
			{
				Console.WriteLine("Waiting for workers...");
				return;
			}

			this.LoadJobDistribution();
			this.CalculateMetrics();
			this.AssignUnassignedJobs();
			this.RebalanceJobs();
		}

		private void RebalanceJobs()
		{
			if (trustfulWorker.Count <= 1)
				return;

			var rebalanceSource = GetSlowestWorker();
			var rebalanceTarget = GetFastestWorker();
			long slowestPerformance = trustfulPerformance[rebalanceSource];
			long fastestPerformance = trustfulPerformance[rebalanceTarget];

			// node is not slow compared to avg or
			// fastest node is not significantly faster
			// do not rebalance
			if (avgWorkerDuration + RebalanceCost > slowestPerformance || fastestPerformance + RebalanceCost > slowestPerformance)
				return;

			int amountJobsToRebalance = Math.Min(assignedWorkerJobs[rebalanceSource].Count / 2, MaxRebalancePerUpdate);
			if (amountJobsToRebalance < 1)
				return;

			Console.WriteLine($"Rebalance from {rebalanceSource} to {rebalanceTarget} with {amountJobsToRebalance} jobs.");
			for(int i = 0; i < amountJobsToRebalance && assignedWorkerJobs[rebalanceSource].Count > 0; i++)
			{
				int jobIndex = assignedWorkerJobs[rebalanceSource].Count - 1;
				string jobName = assignedWorkerJobs[rebalanceSource][jobIndex];
				assignedWorkerJobs[rebalanceSource].RemoveAt(jobIndex);

				serverWorkerJobRepo.UnAssignWorkerToJob(rebalanceSource, jobName);
				serverWorkerJobRepo.AssignWorkerToJob(rebalanceTarget, jobName);
				assignedWorkerJobs[rebalanceTarget].Add(jobName);
				trustfulPerformance[rebalanceTarget] += this.avgJobDuration;
			}

		}

		private void AssignUnassignedJobs()
		{
			foreach(var job in unassignedJobs)
			{
				var worker = GetFastestWorker();

				serverWorkerJobRepo.AssignWorkerToJob(worker, job);
				trustfulPerformance[worker] += this.avgJobDuration;
			}
		}

		private Guid GetSlowestWorker()
		{
			if (trustfulWorker.Count == 1)
				return trustfulWorker[0];

			long slowest = trustfulPerformance.Values.Max();
			return GetWorkerByPerformance(slowest);
		}

		private Guid GetFastestWorker()
		{
			if (trustfulWorker.Count == 1)
				return trustfulWorker[0];

			long fastest = trustfulPerformance.Values.Min();
			return GetWorkerByPerformance(fastest);
		}

		private Guid GetWorkerByPerformance(long performance)
		{
			return trustfulPerformance.First(x => x.Value == performance).Key;
		}

		private void CalculateMetrics()
		{
			long avgJobDuration = 0;
			long executingWorkerCount = 0;
			long avgWorkerDuration = 0;
			foreach (var worker in trustfulWorker)
			{
				var workerPerformance = trustfulPerformance[worker];
				var jobCount = assignedWorkerJobs[worker].Count;
				if(jobCount > 0)
				{
					avgJobDuration += workerPerformance / jobCount;
					executingWorkerCount++;
					avgWorkerDuration += workerPerformance;
				}
			}

			if (executingWorkerCount <= 0)
			{
				this.avgJobDuration = 1;
				this.avgWorkerDuration = 1;
				return;
			}

			avgJobDuration /= executingWorkerCount;
			avgWorkerDuration /= this.trustfulWorker.Count;

			this.avgJobDuration = Math.Max(1, avgJobDuration);
			this.avgWorkerDuration = Math.Max(1, avgWorkerDuration);
		}

		private void LoadJobDistribution()
		{
			assignedWorkerJobs.Clear();
			jobToWorker.Clear();
			unassignedJobs.Clear();

			foreach(var worker in this.trustfulWorker)
			{
				var assignedJobs = serverWorkerJobRepo.GetJobs(worker);
				assignedWorkerJobs.Add(worker, assignedJobs);

				foreach(var job in assignedJobs)
				{
					jobToWorker.Add(job, worker);
				}
			}

			foreach(var job in this.trackedJobs.Keys)
			{ 
				if(!jobToWorker.ContainsKey(job))
				{
					unassignedJobs.Add(job);
				}
			}
		}

		private void UnassignUntrustfulWorker()
		{
			foreach(var worker in this.untrustfulWorker)
			{
				serverWorkerJobRepo.UnAssignWorkerAllJobs(worker);
				trustfulPerformance.Remove(worker);
				serverHeartbeatRepo.Remove(worker);
				serverWorkerListRepo.RemoveWorker(worker);
			}
		}

		private void UpdateWorker()
		{
			untrustfulWorker.Clear();
			trustfulWorker = serverWorkerListRepo.GetAllWorker();

			foreach(var worker in this.trustfulWorker)
			{
				long performance = serverPerformanceRepo.GetPerformance(worker);
				if(trustfulPerformance.ContainsKey(worker))
				{
					trustfulPerformance[worker] = performance;
				}
				else
				{
					trustfulPerformance.Add(worker, performance);
				}

				if(!serverHeartbeatRepo.IsHeartbeatOk(worker))
				{
					untrustfulWorker.Add(worker);
				}
			}

			foreach(var worker in this.untrustfulWorker)
			{
				trustfulWorker.Remove(worker);
			}
		}

		public void AddJob(T job)
		{
			trackedJobs.Add(job.Name, job);
			serverWorkerJobRepo.AddJob(job);
		}
	}
}
