using Common;
using Common.GameDesign;
using Common.Protocol.Combat;
using Common.Protocol.Map;
using CommonServer.Configuration;
using CommonServer.GameDesign;
using CommonServer.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PropManagementService
{

	public class PropManagement
	{
		private readonly PropSpawnConfig config;
		private readonly List<PropStateMessage> props = new List<PropStateMessage>();
		private readonly Dictionary<string, int> respawnTimer = new Dictionary<string, int>();
		private readonly Random random = new Random();
		private readonly DamageCalculator damageCalculator = new DamageCalculator();
		private readonly DamageQueue damageQueue = new DamageQueue();

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
					MaxHealth = config.MaxHealth,
					IsLookingRight = false,
					Type = config.Type
				};

				props.Add(prop);

				SpawnProp(prop);
			}

			damageQueue.OnDamage = OnDamage;
			RedisPubSub.Subscribe<SkillCastMessage>(RedisConfiguration.MapChannelSkillCastPrefix + MapConfiguration.MapName, OnSkillCasted);
		}

		private void OnDamage(DamageInFuture dmg)
		{
			if (dmg.Target.TargetType != SkillCastTargetType.Prop)
			{
				return;
			}

			var effectedProp = props.FirstOrDefault(x => x.Name == dmg.Target.TargetName);
			if (effectedProp == null)
				return;

			effectedProp.Health -= dmg.Damage;
			if (effectedProp.Health < 0)
				effectedProp.Health = 0;

			RedisPubSub.Publish<DamageDoneMessage>(RedisConfiguration.PlayerDamagePrefix + dmg.Caster, new DamageDoneMessage()
			{
				Damage = dmg.Damage,
				Target = dmg.Target
			});
		}

		private void OnSkillCasted(RedisChannel channel, SkillCastMessage msg)
		{
			if (msg.Target.TargetType == SkillCastTargetType.Prop)
			{
				var effectedProp = props.FirstOrDefault(x => x.Name == msg.Target.TargetName);
				if (effectedProp == null)
					return;
			}

			damageQueue.Enqueue(new DamageInFuture()
			{
				Caster = msg.Caster,
				Damage = damageCalculator.GetDamage(),
				Target = msg.Target,
				WaitDuration = msg.DurationInMs - (int)(DateTime.UtcNow - new DateTime(msg.ServerTime)).TotalMilliseconds
			});
		}

		private void SpawnProp(PropStateMessage prop)
		{
			prop.ServerTime = DateTime.UtcNow.Ticks;
			prop.Position = GetRandomPosition();
			prop.Health = config.MaxHealth;

			respawnTimer[prop.Name] = config.RespawnTimeInMs;
		}

		private void HandleRespawn(int timeInMs)
		{
			foreach (var prop in props)
			{
				if (prop.Health > 0)
					continue;

				respawnTimer[prop.Name] = respawnTimer[prop.Name] - timeInMs;

				if (respawnTimer[prop.Name] <= 0)
				{
					SpawnProp(prop);
				}
			}
		}

		public void Update(int timeInMs)
		{
			HandleRespawn(timeInMs);
			damageQueue.Update(timeInMs);

			foreach (var prop in props)
			{
				RedisPubSub.Publish<PropStateMessage>(RedisConfiguration.MapChannelNewPropStatePrefix + MapConfiguration.MapName, prop);
			}
		}
	}
}
