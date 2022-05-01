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
using System.Numerics;
using System.Threading.Tasks;

namespace EnemyWorkerService
{
	public class EnemyAI
	{
		private readonly string name;
		private readonly EntityStats stats;
		private EnemyAIConfig aIConfig;
		private CooldownManagement cooldownManagement = new CooldownManagement();
		private readonly DamageCalculator damageCalculator = new DamageCalculator();
		private readonly DelayQueue<SkillCastMessage> delayQueue = new DelayQueue<SkillCastMessage>();
		private List<PlayerStateMessage> worldState;
		private float castDelay = 0;

		public EnemyAI(string name, EntityStats stats, EnemyAIConfig aIConfig)
		{
			this.name = name;
			this.stats = stats;
			this.aIConfig = aIConfig;
			this.delayQueue.OnExecute = DamageAoE;
		}

		private async Task DamageAoE(DelayQueueEntry<SkillCastMessage> entry)
		{
			foreach (var pc in worldState)
			{
				Vector2 skillSpacePosition = pc.Position - entry.Data.Position;

				var sc = AoESkillsLoader.SkillCollisions[entry.Data.Type];
				if (!sc.IsHit(skillSpacePosition))
					continue;

				var damageInfo = damageCalculator.GetDamage(this.stats, pc.Stats, GameDesignConfiguration.Skills.SkillTable[entry.Data.Type].GetStats(aIConfig.Skills[entry.Data.Type]));

				RedisPubSub.Publish<DamageMessage>(RedisConfiguration.PlayerDamagePrefix + pc.Name, new DamageMessage()
				{
					DamageInfo = damageInfo,
					Target = new SkillTarget()
					{
						TargetName = pc.Name,
						TargetPosition = pc.Position,
						TargetType = SkillCastTargetType.Position
					}
				});
			}
		}

		public async Task Update(int timeInMs, List<PlayerStateMessage> world)
		{
			castDelay -= timeInMs;
			worldState = world;
			await delayQueue.Update(timeInMs);

			var castPrio = GetNextCast();

			if (castPrio == null)
				return;

			var target = new SkillTarget()
			{
				TargetName = "AoE",
				TargetPosition = new System.Numerics.Vector2(66.81f, -41.32f),
				TargetType = SkillCastTargetType.Position
			};

			var skillCastMessage = new SkillCastMessage()
			{
				Caster = this.name,
				Type = castPrio.Type,
				Position = target.TargetPosition,
				ServerTime = DateTime.UtcNow.Ticks,
				Target = target,
				CasterStats = this.stats
			};
			RedisPubSub.Publish<SkillCastMessage>(RedisConfiguration.MapChannelSkillCastPrefix + MapConfiguration.MapName, skillCastMessage);

			delayQueue.Enqueue(new DelayQueueEntry<SkillCastMessage>()
			{
				Data = skillCastMessage,
				WaitDuration = GameDesignConfiguration.SkillIndicatorDelay[castPrio.Type]
			});

			cooldownManagement.Cast(castPrio.Type);
			castDelay = castPrio.DelayForNextSkill;
		}

		private EnemyAICastPriority GetNextCast()
		{
			if (castDelay > 0)
				return null;

			foreach (var castPrio in this.aIConfig.CastPriority)
			{
				if (cooldownManagement.CanCast(castPrio.Type, aIConfig.Skills[castPrio.Type]))
				{
					return castPrio;
				}
			}

			return null;
		}
	}
}
