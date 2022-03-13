using CommonServer.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonServer.PrimarySecondary
{
	public class PrimarySecondaryServer
	{
		public string RedisKeyNamePrefix { get; private set; }
		public string RedisKeyPrimary { get; private set; }
		public string RedisKeyHeartbeatPrefix { get; private set; }
		public Guid Id { get; private set; }
		public int UpdateDelay { get; private set; }
		public int MaxPrimaryTrustDelay { get; private set; }
		public Action OnPrimaryUpdate { get; set; }

		public bool IsRunning { get; private set; } = false;
		private Task updateTask;
		private string lastPrimary;

		public PrimarySecondaryServer(Action onPrimaryUpdate, string redisKeyNamePrefix, Guid id, int updateDelay = 100, int maxPrimaryTrustDelay = 2000)
		{
			this.OnPrimaryUpdate = onPrimaryUpdate;
			this.Id = id;
			this.SetRedisKeyNamePrefix(redisKeyNamePrefix);

			this.UpdateDelay = updateDelay;
			this.MaxPrimaryTrustDelay = maxPrimaryTrustDelay;

			this.Start();
		}

		private void SetRedisKeyNamePrefix(string redisKeyNamePrefix)
		{
			RedisKeyNamePrefix = redisKeyNamePrefix;
			RedisKeyPrimary = $"{RedisKeyNamePrefix}_Primary";
			RedisKeyHeartbeatPrefix = $"{RedisKeyNamePrefix}_Heartbeat_";
		}

		private string GetHeartbeatKey(Guid id)
		{
			return $"{RedisKeyHeartbeatPrefix}{id}";
		}

		public void Start()
		{
			if (IsRunning)
				return;

			Console.WriteLine($"Start with id - {this.Id}");
			if (updateTask != null)
			{
				updateTask.Wait(this.UpdateDelay);
			}

			IsRunning = true;
			updateTask = Task.Run(Update);
		}

		public void Stop()
		{
			if (!IsRunning)
				return;

			Console.WriteLine($"Stop with id - {this.Id}");
			IsRunning = false;
		}

		private async Task Update()
		{
			while (IsRunning)
			{
				UpdateHeartbeat();
				bool isPrimary = this.UpdatePrimary();

				if (isPrimary && OnPrimaryUpdate != null)
				{
					OnPrimaryUpdate();
				}

				await Task.Delay(this.UpdateDelay);
			}
		}

		private void UpdateHeartbeat()
		{
			RedisKV.Set(this.GetHeartbeatKey(this.Id), DateTime.UtcNow.Ticks.ToString());
		}

		private bool IsHeartbeatOk(Guid id)
		{
			string hbTicksStr = RedisKV.Get(this.GetHeartbeatKey(id));
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

		private bool UpdatePrimary()
		{
			bool isPrimary = false;
			string primary = RedisKV.Get(RedisKeyPrimary);
			if (string.IsNullOrEmpty(primary))
			{
				Console.WriteLine($"No Primary start race");
				RedisKV.SetIfNotExists(RedisKeyPrimary, this.Id.ToString());
			}
			else
			{
				isPrimary = primary == this.Id.ToString();

				if (!isPrimary)
				{
					if (!this.IsHeartbeatOk(new Guid(primary)))
					{
						Console.WriteLine($"Primary is not trustworthy - {primary}");
						this.ForcefullyRemovePrimary();
					}
				}

				if (lastPrimary != primary)
				{
					Console.WriteLine($"New Primary is {primary}");

					if (isPrimary)
					{
						Console.WriteLine($"I am the primary!");
					}
					lastPrimary = primary;
				}
			}

			return isPrimary;
		}

		private void ForcefullyRemovePrimary()
		{
			RedisKV.Remove(this.RedisKeyPrimary);
		}
	}
}
