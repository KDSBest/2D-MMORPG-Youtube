using Common.GameDesign;
using Common.Protocol.Inventory;
using Common.Protocol.PlayerEvent;
using CommonServer.Configuration;
using CommonServer.CosmosDb;
using CommonServer.CosmosDb.Model;
using CommonServer.Redis;
using CommonServer.Redis.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DailyLoginService
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			UserLastLoginRepository repo = new UserLastLoginRepository();
			InventoryEventRepository inventoryEventRepo = new InventoryEventRepository();

			while (true)
			{
				var val = RedisQueue.Dequeue<LoginQueueItem>(RedisConfiguration.LoginQueue);
				if (val == null)
				{
					Thread.Sleep(100);
					continue;
				}

				Console.WriteLine($"Processing login of {val.PlayerId}.");

				var lastLogin = await repo.GetAsync(val.PlayerId);
				if (lastLogin == null || lastLogin.LastLogin.Date != DateTime.Now.Date)
				{
					Console.WriteLine($"Daily Login for {val.PlayerId}.");

					await inventoryEventRepo.GiveLoot(val.PlayerId, LoottableConfiguration.Daily, PlayerEventType.DailyLogin);

					await repo.SaveAsync(new UserLastLogin()
					{
						Id = val.PlayerId,
						LastLogin = val.LoginTime
					}, val.PlayerId);
				}
			}
		}
	}
}
