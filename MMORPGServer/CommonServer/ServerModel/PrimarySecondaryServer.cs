using CommonServer.ServerModel.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonServer.ServerModel
{
	public class PrimarySecondaryServer : Server
	{
		public string RedisKeyNamePrefix { get; private set; }
		public Action OnPrimaryUpdate { get; set; }

		public int MaxPrimaryTrustDelay { get; }
		public Action OnPrimaryStart { get; set; }

		private string lastPrimary;

		private RedisServerHeartbeatRepository serverHeartbeatRepo;
		private RedisServerPrimaryRepository serverPrimaryRepo;

		public PrimarySecondaryServer(string redisKeyNamePrefix) : this(redisKeyNamePrefix, Guid.NewGuid())
		{

		}

		public PrimarySecondaryServer(string redisKeyNamePrefix, Guid id, int updateDelay = 100, int maxPrimaryTrustDelay = 2000) : base(id, updateDelay)
		{
			MaxPrimaryTrustDelay = maxPrimaryTrustDelay;
			RedisKeyNamePrefix = redisKeyNamePrefix;

			serverHeartbeatRepo = new RedisServerHeartbeatRepository(redisKeyNamePrefix, maxPrimaryTrustDelay);
			serverPrimaryRepo = new RedisServerPrimaryRepository(redisKeyNamePrefix);
		}

		protected override async Task Update()
		{
				serverHeartbeatRepo.UpdateHeartbeat(this.Id);
				bool isPrimary = this.UpdatePrimary();

				if (isPrimary && OnPrimaryUpdate != null)
				{
					OnPrimaryUpdate();
				}
		}

		private bool UpdatePrimary()
		{
			bool isPrimary = false;
			string primary = serverPrimaryRepo.GetPrimary(); 
			if (string.IsNullOrEmpty(primary))
			{
				Console.WriteLine($"No Primary start race");
				serverPrimaryRepo.TrySetPrimary(this.Id);
			}
			else
			{
				isPrimary = primary == this.Id.ToString();

				if (!isPrimary)
				{
					if (!serverHeartbeatRepo.IsHeartbeatOk(new Guid(primary)))
					{
						Console.WriteLine($"Primary is not trustworthy - {primary}");
						serverPrimaryRepo.ForcefullyRemovePrimary();
					}
				}

				if (lastPrimary != primary)
				{
					Console.WriteLine($"New Primary is {primary}");

					if (isPrimary)
					{
						Console.WriteLine($"I am the primary!");
						if(OnPrimaryStart != null)
						{
							OnPrimaryStart();
						}
					}
					lastPrimary = primary;
				}
			}

			return isPrimary;
		}
	}
}
