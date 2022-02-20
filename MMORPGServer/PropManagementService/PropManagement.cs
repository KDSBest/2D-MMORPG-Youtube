using Common;
using Common.GameDesign;
using Common.Protocol.Combat;
using Common.Protocol.Inventory;
using Common.Protocol.Map;
using Common.Protocol.PlayerEvent;
using CommonServer.Configuration;
using CommonServer.CosmosDb;
using CommonServer.CosmosDb.Model;
using CommonServer.GameDesign;
using CommonServer.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace PropManagementService
{

	public class PropManagement
	{
		private readonly PropSpawnConfig config;
		private readonly List<PropStateMessage> props = new List<PropStateMessage>();
		private readonly Dictionary<string, int> respawnTimer = new Dictionary<string, int>();
		private readonly Dictionary<string, EntityStats> propStats = new Dictionary<string, EntityStats>();
		private readonly Random random = new Random();
		private readonly DamageCalculator damageCalculator = new DamageCalculator();
		private readonly DamageQueue damageQueue = new DamageQueue();
		private readonly InventoryEventRepository inventoryEventRepo = new InventoryEventRepository();

		public PropManagement(PropSpawnConfig config)
		{
			this.config = config;
		}

		private Vector2 GetRandomPosition()
		{
			return config.SpawnStart + ((config.SpawnEnd - config.SpawnStart) * (float)random.NextDouble());
		}

		public void Initialize()
		{
			for (int i = 0; i < config.SpawnCount; i++)
			{
				var prop = new PropStateMessage()
				{
					Name = config.PropPrefix + i,
					Animation = 0,
					Stats = new EntityStats()
					{
						MaxHP = config.Stats.MaxHP,
						HP = config.Stats.MaxHP
					},
					IsLookingRight = false,
					Type = config.Type
				};

				props.Add(prop);
				propStats.Add(prop.Name, config.Stats);

				SpawnProp(prop);
			}

			damageQueue.OnDamage = OnDamage;
			RedisPubSub.Subscribe<SkillCastMessage>(RedisConfiguration.MapChannelSkillCastPrefix + MapConfiguration.MapName, OnSkillCasted);
		}

		private async Task OnDamage(DamageInFuture dmg)
		{
			if (dmg.Target.TargetType != SkillCastTargetType.SingleTarget)
			{
				return;
			}

			var effectedProp = props.FirstOrDefault(x => x.Name == dmg.Target.TargetName);
			if (effectedProp == null)
				return;

			// Prop is already dead, without this a bug when the prop is attacked multiple
			// times at the same time occurse
			if (effectedProp.Stats.HP <= 0)
				return;

				effectedProp.Stats.HP -= dmg.DamageInfo.Damage;
			if (effectedProp.Stats.HP < 0)
				effectedProp.Stats.HP = 0;

			if(effectedProp.Stats.HP == 0)
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
			if (msg.Target.TargetType == SkillCastTargetType.SingleTarget)
			{
				var effectedProp = props.FirstOrDefault(x => x.Name == msg.Target.TargetName);
				propName = effectedProp.Name;
				if (effectedProp == null)
					return;
			}

			var damageInfo = damageCalculator.GetDamage(msg.CasterStats, propStats[propName], GameDesignConfiguration.Skills.SkillTable[msg.Type].GetStats(1));

			damageQueue.Enqueue(new DamageInFuture()
			{
				Caster = msg.Caster,
				DamageInfo = damageInfo,
				Target = msg.Target,
				WaitDuration = GameDesignConfiguration.AnimationDelay[msg.Type] - (int)(DateTime.UtcNow - new DateTime(msg.ServerTime)).TotalMilliseconds
			});
		}

		private void SpawnProp(PropStateMessage prop)
		{
			prop.ServerTime = DateTime.UtcNow.Ticks;
			prop.Position = GetRandomPosition();
			prop.Stats.HP = config.Stats.MaxHP;

			respawnTimer[prop.Name] = config.RespawnTimeInMs;
		}

		private void HandleRespawn(int timeInMs)
		{
			foreach (var prop in props)
			{
				if (prop.Stats.HP > 0)
					continue;

				respawnTimer[prop.Name] = respawnTimer[prop.Name] - timeInMs;

				if (respawnTimer[prop.Name] <= 0)
				{
					SpawnProp(prop);
				}
			}
		}

		public async Task Update(int timeInMs)
		{
			HandleRespawn(timeInMs);
			await damageQueue.Update(timeInMs);

			foreach (var prop in props)
			{
				RedisPubSub.Publish<PropStateMessage>(RedisConfiguration.MapChannelNewPropStatePrefix + MapConfiguration.MapName, prop);
			}
		}
	}
}
