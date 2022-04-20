using Common.GameDesign;
using Common.IoC;
using Common.Protocol.Map;
using Common.PublishSubscribe;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Remoting
{
	public class RemoteTargeting
	{
		private Dictionary<string, Vector2> targetLocations = new Dictionary<string, Vector2>();
		private IPubSub pubsub;

		public RemoteTargeting(string pubSubName)
		{
			DILoader.Initialize();

			pubsub = DI.Instance.Resolve<IPubSub>();
			pubsub.Subscribe<EnemyStateMessage>(OnEnemyState, pubSubName);
			pubsub.Subscribe<PlayerStateMessage>(OnPlayerState, pubSubName);
		}

		private void OnEnemyState(EnemyStateMessage state)
		{
			targetLocations[state.Name] = new Vector2(state.Position.X, state.Position.Y);
		}

		private void OnPlayerState(PlayerStateMessage state)
		{
			targetLocations[state.Name] = new Vector2(state.Position.X, state.Position.Y);
		}

		public bool CheckValidTargeting(SkillTarget target)
		{
			switch (target.TargetType)
			{
				case SkillCastTargetType.SingleTarget:
					return targetLocations.ContainsKey(target.TargetName);
				case SkillCastTargetType.Position:
					return true;
			}

			return false;
		}

		public Vector3 GetTargetPosition(SkillTarget target)
		{
			switch (target.TargetType)
			{
				case SkillCastTargetType.SingleTarget:
					if (!targetLocations.ContainsKey(target.TargetName))
					{
						return Vector3.zero;
					}

					return targetLocations[target.TargetName];
				case SkillCastTargetType.Position:
					return new Vector3(target.TargetPosition.X, target.TargetPosition.Y, 0);
			}

			return Vector3.zero;
		}
	}
}
