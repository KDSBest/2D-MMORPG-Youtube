using Common;
using Common.GameDesign;
using CommonServer.CosmosDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PropManagementService
{

	public class Program
	{
		public static async Task Main(string[] args)
		{
			Console.WriteLine("Initialize CosmosDb Connection.");
			var repo = new InventoryEventRepository();

			Console.WriteLine($"Loading Map... {MapConfiguration.MapName}.");
			var spawns = new List<PropSpawnConfig>()
			{
				new PropSpawnConfig()
				{
					PropPrefix = "F*",
					MaxHealth = 30,
					SpawnCount = 10,
					SpawnStart = new System.Numerics.Vector2(-10, -44.5f),
					SpawnEnd = new System.Numerics.Vector2(44, -44.5f),
					RespawnTimeInMs = 10000,
					Type = PropType.Flower
				}
			};

			var propManager = spawns.Select(x => new PropManagement(x)).ToList();
			propManager.ForEach(x => x.Initialize());

			Console.WriteLine($"Prop Management Started.");
			while (true)
			{
				propManager.ForEach(async x =>
				{
					await x.Update(100);
				});
				await Task.Delay(100);
			}
		}
	}
}
