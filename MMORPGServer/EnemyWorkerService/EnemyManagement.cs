using Common;
using Common.GameDesign;
using Common.Protocol.Combat;
using Common.Protocol.Map;
using Common.Protocol.PlayerEvent;
using CommonServer.Configuration;
using CommonServer.CosmosDb;
using CommonServer.GameDesign;
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

		public EnemyManagement()
		{
			RedisPubSub.Subscribe<SkillCastMessage>(RedisConfiguration.MapChannelSkillCastPrefix + MapConfiguration.MapName, OnSkillCasted);
		}

		private Vector2 GetRandomPosition(EnemySpawnConfig config)
		{
			return config.SpawnStart + ((config.SpawnEnd - config.SpawnStart) * (float)random.NextDouble());
		}

		public void Initialize(EnemyLoadEntry enemy)
		{
			if (enemies.ContainsKey(enemy.Name))
				return;

			var prop = new EnemyStateMessage()
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

			enemies.AddOrUpdate(enemy.Name, prop, (key, val) => prop);
			SpawnProp(prop, enemy.Config);

			damageQueue.OnDamage = OnDamage;
		}

		private async Task OnDamage(DamageInFuture dmg)
		{
			if (dmg.Target.TargetType != SkillCastTargetType.SingleTarget)
			{
				return;
			}

			if (!enemies.ContainsKey(dmg.Target.TargetName))
				return;

			var effectedProp = enemies[dmg.Target.TargetName];
			if (effectedProp == null)
				return;

			// Prop is already dead, without this a bug when the prop is attacked multiple
			// times at the same time occurse
			if (effectedProp.Stats.HP <= 0)
				return;

			effectedProp.Stats.HP -= dmg.DamageInfo.Damage;
			if (effectedProp.Stats.HP < 0)
				effectedProp.Stats.HP = 0;

			if (effectedProp.Stats.HP == 0)
			{
				await inventoryEventRepo.GiveLoot(dmg.Caster, LoottableConfiguration.Prop[effectedProp.Type], PlayerEventType.PropKill);

				RedisPubSub.Publish<ExpMessage>(RedisConfiguration.PlayerExpPrefix + dmg.Caster, new ExpMessage()
				{
					ExpGain = ExpTable.Prop[effectedProp.Type]
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

			respawnTimer[prop.Name] = config.RespawnTimeInMs;
		}

		private void HandleRespawn(int timeInMs, EnemyLoadEntry enemy)
		{
			string enemyName = enemy.Name;
			if (!enemies.ContainsKey(enemyName))
				return;

			var prop = enemies[enemyName];
			if (prop.Stats.HP > 0)
				return;

			respawnTimer[prop.Name] = respawnTimer[prop.Name] - timeInMs;

			if (respawnTimer[prop.Name] <= 0)
			{
				SpawnProp(prop, enemy.Config);
			}
		}

		public async Task Update(int timeInMs, EnemyLoadEntry enemy)
		{
			Initialize(enemy);
			HandleRespawn(timeInMs, enemy);
			await damageQueue.Update(timeInMs);

			SendToMapServer(enemy);
		}

		private void SendToMapServer(EnemyLoadEntry enemy)
		{
			if (!enemies.ContainsKey(enemy.Name))
				return;
			var prop = enemies[enemy.Name];
			RedisPubSub.Publish<EnemyStateMessage>(RedisConfiguration.MapChannelNewPropStatePrefix + MapConfiguration.MapName, prop);
		}
	}
}
