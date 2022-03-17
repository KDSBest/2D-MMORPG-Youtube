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

			var prop = enemyStateRepo.Get(enemy.Name);

			if (prop == null)
			{
				prop = new EnemyStateMessage()
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
				SpawnProp(prop, enemy.Config);
			}

			enemies.AddOrUpdate(enemy.Name, prop, (key, val) => prop);
			InitializeRespawnTimer(prop, enemy.Config);
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

		private void SpawnProp(EnemyStateMessage prop, EnemySpawnConfig config)
		{
			prop.ServerTime = DateTime.UtcNow.Ticks;
			prop.Position = GetRandomPosition(config);
			prop.Stats.HP = config.Stats.MaxHP;
		}

		private void InitializeRespawnTimer(EnemyStateMessage enemy, EnemySpawnConfig config)
		{
			respawnTimer[enemy.Name] = config.RespawnTimeInMs;
		}

		private void HandleRespawn(int timeInMs, EnemyJob enemyJob)
		{
			string enemyName = enemyJob.Name;
			if (!enemies.ContainsKey(enemyName))
				return;

			var enemy = enemies[enemyName];
			if (enemy.Stats.HP > 0)
				return;

			respawnTimer[enemy.Name] = respawnTimer[enemy.Name] - timeInMs;

			if (respawnTimer[enemy.Name] <= 0)
			{
				SpawnProp(enemy, enemyJob.Config);
				InitializeRespawnTimer(enemy, enemyJob.Config);
			}
		}

		public async Task Update(int timeInMs, EnemyJob enemy)
		{
			Initialize(enemy);
			HandleRespawn(timeInMs, enemy);
			await damageQueue.Update(timeInMs);

			SaveStateToRedis(enemy.Name);
			SendToMapServer(enemy);
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
