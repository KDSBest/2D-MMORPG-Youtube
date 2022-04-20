using Common;
using Common.GameDesign;
using Common.Protocol.Combat;
using Common.Protocol.Map;
using CommonServer.Configuration;
using CommonServer.GameDesign;
using CommonServer.Redis;
using CommonServer.WorldManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnemyWorkerService
{
	public class EnemyAI
	{
		private readonly string name;
		private readonly EntityStats stats;
		private EnemyAIConfig aIConfig;
		private CooldownManagement cooldownManagement = new CooldownManagement();
		private readonly DamageCalculator damageCalculator = new DamageCalculator();


		public EnemyAI(string name, EntityStats stats, EnemyAIConfig aIConfig)
		{
			this.name = name;
			this.stats = stats;
			this.aIConfig = aIConfig;
		}

		public void Update(int timeInMs, List<PlayerStateMessage> world)
		{
			var nextCast = GetNextCast();

			if (!nextCast.HasValue)
				return;

			if (world.Count > 0)
			{
				var target = new SkillTarget()
				{
					TargetName = world[0].Name,
					TargetPosition = world[0].Position,
					TargetType = SkillCastTargetType.SingleTarget
				};

				RedisPubSub.Publish<SkillCastMessage>(RedisConfiguration.MapChannelSkillCastPrefix + MapConfiguration.MapName, new SkillCastMessage()
				{
					Caster = this.name,
					Type = nextCast.Value,
					Position = world[0].Position,
					ServerTime = DateTime.UtcNow.Ticks,
					Target = target,
					CasterStats = this.stats
				});

				var damageInfo = damageCalculator.GetDamage(this.stats, world[0].Stats, GameDesignConfiguration.Skills.SkillTable[nextCast.Value].GetStats(aIConfig.Skills[nextCast.Value]));

				RedisPubSub.Publish<DamageMessage>(RedisConfiguration.PlayerDamagePrefix + world[0].Name, new DamageMessage()
				{
					DamageInfo = damageInfo,
					Target = target,
				});

				cooldownManagement.Cast(nextCast.Value);
			}
		}

		private SkillCastType? GetNextCast()
		{
			foreach (var cast in this.aIConfig.CastOrder)
			{
				if (cooldownManagement.CanCast(cast, aIConfig.Skills[cast]))
				{
					return cast;
				}
			}

			return null;
		}
	}
}
