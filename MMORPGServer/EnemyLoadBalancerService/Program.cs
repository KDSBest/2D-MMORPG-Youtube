using Common;
using Common.GameDesign;
using CommonServer.CosmosDb;
using CommonServer.GameDesign;
using CommonServer.ServerModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
			var spawns = new List<EnemySpawnConfig>()
			{
				new EnemySpawnConfig()
				{
					Prefix = "F*",
					Stats = new EntityStats()
					{
						Attack = 1,
						Defense = 1,
						MAttack = 1,
						MDefense = 1,
						Level = 1,
						MaxHP = 25
					},
					SpawnCount = 10,
					SpawnStart = new System.Numerics.Vector2(-10, -44.5f),
					SpawnEnd = new System.Numerics.Vector2(44, -44.5f),
					RespawnTimeInMs = 10000,
					Type = EnemyType.Flower,
					AIConfig = null
				},
				new EnemySpawnConfig()
				{
					Prefix = "E*",
					Stats = new EntityStats()
					{
						Attack = 1,
						Defense = 1,
						MAttack = 1,
						MDefense = 1,
						Level = 1,
						MaxHP = 25
					},
					SpawnCount = 2,
					SpawnStart = new System.Numerics.Vector2(-10, -44.5f),
					SpawnEnd = new System.Numerics.Vector2(44, -44.5f),
					RespawnTimeInMs = 10000,
					Type = EnemyType.SimpleEnemy,
					AIConfig = new EnemyAIConfig()
					{
						Skills = new Dictionary<SkillCastType, int> ()
						{
							{SkillCastType.LightningBolt, 1 }
						},
						CastOrder = new List<SkillCastType>
						{
							SkillCastType.LightningBolt
						}
					}
				}
			};

			string servername = $"E*{MapConfiguration.MapName}";
			Console.WriteLine($"Start Load Balancing Server {servername}.");
			var server = new LoadBalancerServer<EnemyJob>(servername);
			foreach (var spawn in spawns)
			{

				for (int i = 0; i < spawn.SpawnCount; i++)
				{
					string enemyName = $"{spawn.Prefix}{i + 1}";
					// Console.WriteLine($"Add {enemyName} to {servername}.");

					server.AddJob(new EnemyJob()
					{
						Name = enemyName,
						Config = spawn
					});
				}

			}
			server.Start();

			while (server.IsRunning)
			{
				await Task.Delay(100);
			}
		}
	}
}
