using Common.GameDesign;
using Common.IoC;
using Common.Protocol.Combat;
using Common.PublishSubscribe;
using System;
using UnityEngine;

namespace Assets.Scripts.Character
{
	[Serializable]
	public class PlayerSkill
	{
		private IPubSub pubsub;
		public SkillCastType Type;
		public RectTransform CooldownBackground;
		private int Cooldown = 0;
		private float rectHeight = 100;
		private float maxCastRange = 10;
		public LayerMask LayerMask;

		public PlayerSkill(SkillCastType type)
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
			this.Type = type;
		}

		public void Update(int timeInMs)
		{
			Cooldown -= timeInMs;
			if (Cooldown < 0)
				Cooldown = 0;

			float fillAmount = (float)(GameDesignConfiguration.Cooldowns[Type] - Cooldown) / (float)GameDesignConfiguration.Cooldowns[Type];
			CooldownBackground.sizeDelta = new Vector2(CooldownBackground.sizeDelta.x, fillAmount * rectHeight);
		}

		public void Cast(Vector3 position, Vector3 lookDir)
		{
			if (Cooldown > 0)
				return;

			var hit = Physics2D.Raycast(position, lookDir, maxCastRange, LayerMask);
			if (hit.collider == null)
				return;

			Cooldown = GameDesignConfiguration.Cooldowns[Type];

			pubsub.Publish<ReqSkillCastMessage>(new ReqSkillCastMessage()
			{
				Position = new System.Numerics.Vector2(position.x, position.y),
				Type = Type,
				Target = new SkillTarget()
				{
					TargetName = hit.transform.gameObject.name,
					TargetType = SkillCastTargetType.SingleTarget
				}
			});
		}
	}
}
