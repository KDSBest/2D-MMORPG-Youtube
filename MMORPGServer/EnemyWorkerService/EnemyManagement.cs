using Common;
using Common.GameDesign;
using Common.Protocol.Combat;
using Common.Protocol.Map;
using Common.Protocol.PlayerEvent;
using CommonServer.Configuration;
using CommonServer.CosmosDb;
using CommonServer.GameDesign;
using CommonServer.GameDesign.Repos;
using CommonServer.Redis;
using CommonServer.WorldManagement;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace EnemyWorkerService
{

	public class EnemyManagement
	{
		private readonly ConcurrentDictionary<string, EnemyStateMessage> enemies = new ConcurrentDictionary<string, EnemyStateMessage>();
		private readonly ConcurrentDictionary<string, EnemyAI> enemyAIs = new ConcurrentDictionary<string, EnemyAI>();
		private readonly ConcurrentDictionary<string, int> respawnTimer = new ConcurrentDictionary<string, int>();
		private readonly Random random = new Random();
		private readonly DamageCalculator damageCalculator = new DamageCalculator();
		private readonly DamageQueue damageQueue = new DamageQueue();
		private readonly InventoryEventRepository inventoryEventRepo = new InventoryEventRepository();
		private readonly EnemyStateRepo enemyStateRepo = new EnemyStateRepo();

		public EnemyManagement()
		{
			damageQueue.OnDamage = OnDamage;
			RedisPubSub.Subscribe<SkillCastMessage>(RedisConfiguration.MapChannelSkillCastPrefix + MapConfiguration.MapName, OnSkillCasted);
		}

		private Vector2 GetRandomPosition(EnemySpawnConfig config)
		{
			return config.SpawnStart + ((config.SpawnEnd - config.SpawnStart) * (float)random.NextDouble());
		}

		public void Initialize(EnemyJob enemy)
		{
			if (enemies.ContainsKey(enemy.Name))
				return;

			var enemyState = enemyStateRepo.Get(enemy.Name);

			if (enemyState == null)
			{
				enemyState = new EnemyStateMessage()
				{
					Name = enemy.Name,
					Animation = 0,
					Stats = new EntityStats()
					{
						MaxHP = enemy.Config.Stats.MaxHP,
						HP = enemy.Config.Stats.MaxHP
					},
					IsLookingRight = false,
					Type = enemy.Config.Type
				};
				SpawnEnemy(enemyState, enemy.Config);
			}

			enemies.AddOrUpdate(enemy.Name, enemyState, (key, val) => enemyState);

			if(enemy.Config.AIConfig != null)
			{
				var ai = new EnemyAI(enemy.Name, enemy.Config.Stats, enemy.Config.AIConfig);
				enemyAIs.AddOrUpdate(enemy.Name, ai, (k, oldAi) => ai);
			}
			InitializeRespawnTimer(enemyState, enemy.Config);
		}

		private async Task OnDamage(DamageInFuture dmg)
		{
			if (dmg.Target.TargetType != SkillCastTargetType.SingleTarget)
			{
				return;
			}

			if (!enemies.ContainsKey(dmg.Target.TargetName))
				return;

			var effectedEnemy = enemies[dmg.Target.TargetName];
			if (effectedEnemy == null)
				return;

			// Prop is already dead, without this a bug when the prop is attacked multiple
			// times at the same time occurse
			if (effectedEnemy.Stats.HP <= 0)
				return;

			effectedEnemy.Stats.HP -= dmg.DamageInfo.Damage;
			if (effectedEnemy.Stats.HP < 0)
				effectedEnemy.Stats.HP = 0;

			if (effectedEnemy.Stats.HP == 0)
			{
				await inventoryEventRepo.GiveLoot(dmg.Caster, LoottableConfiguration.Prop[effectedEnemy.Type], PlayerEventType.PropKill);

				RedisPubSub.Publish<ExpMessage>(RedisConfiguration.PlayerExpPrefix + dmg.Caster, new ExpMessage()
				{
					ExpGain = ExpTable.Prop[effectedEnemy.Type]
				});
			}

			RedisPubSub.Publish<DamageMessage>(RedisConfiguration.PlayerDamagePrefix + dmg.Caster, new DamageMessage()
			{
				DamageInfo = dmg.DamageInfo,
				Target = dmg.Target,
			});
		}

		private void OnSkillCasted(RedisChannel channel, SkillCastMessage msg)
		{
			string propName = string.Empty;
			EnemyStateMessage effectedProp = null;
			if (msg.Target.TargetType == SkillCastTargetType.SingleTarget)
			{
				if (!enemies.ContainsKey(msg.Target.TargetName))
					return;

				effectedProp = enemies[msg.Target.TargetName];
				propName = effectedProp.Name;
			}

			if (effectedProp == null)
				return;

			var damageInfo = damageCalculator.GetDamage(msg.CasterStats, effectedProp.Stats, GameDesignConfiguration.Skills.SkillTable[msg.Type].GetStats(1));

			damageQueue.Enqueue(new DamageInFuture()
			{
				Caster = msg.Caster,
				DamageInfo = damageInfo,
				Target = msg.Target,
				WaitDuration = GameDesignConfiguration.AnimationDelay[msg.Type] - (int)(DateTime.UtcNow - new DateTime(msg.ServerTime)).TotalMilliseconds
			});
		}

		private void SpawnEnemy(EnemyStateMessage prop, EnemySpawnConfig config)
		{
			prop.ServerTime = DateTime.UtcNow.Ticks;
			prop.Position = GetRandomPosition(config);
			prop.Stats.HP = config.Stats.MaxHP;
		}

		private void InitializeRespawnTimer(EnemyStateMessage enemy, EnemySpawnConfig config)
		{
			respawnTimer[enemy.Name] = config.RespawnTimeInMs;
		}

		private void HandleRespawn(int timeInMs, EnemyJob enemyJob, EnemyStateMessage enemyStats)
		{
			if (enemyStats == null)
				return;

			if (enemyStats.Stats.HP > 0)
				return;

			respawnTimer[enemyStats.Name] = respawnTimer[enemyStats.Name] - timeInMs;

			if (respawnTimer[enemyStats.Name] <= 0)
			{
				SpawnEnemy(enemyStats, enemyJob.Config);
				InitializeRespawnTimer(enemyStats, enemyJob.Config);
			}
		}

		public async Task Update(int timeInMs, EnemyJob enemyJob, List<PlayerStateMessage> world)
		{
			Initialize(enemyJob);

			string enemyName = enemyJob.Name;
			var enemyStats = enemies[enemyName];
			if (enemyStats.Stats.HP > 0)
			{
				if(enemyJob.Config.AIConfig != null && enemyAIs.ContainsKey(enemyName))
				{
					enemyAIs[enemyName].Update(timeInMs, world);
				}
			}
			else
			{
				HandleRespawn(timeInMs, enemyJob, enemyStats);
			}

			await damageQueue.Update(timeInMs);



			SaveStateToRedis(enemyName);
			SendToMapServer(enemyJob);
		}

		private void SaveStateToRedis(string name)
		{
			if (!enemies.ContainsKey(name))
				return;

			var enemy = enemies[name];

			enemyStateRepo.Save(name, enemy);
		}

		private void SendToMapServer(EnemyJob enemyJob)
		{
			if (!enemies.ContainsKey(enemyJob.Name))
				return;
			var enemy = enemies[enemyJob.Name];
			RedisPubSub.Publish<EnemyStateMessage>(RedisConfiguration.MapChannelNewPropStatePrefix + MapConfiguration.MapName, enemy);
		}
	}
}
