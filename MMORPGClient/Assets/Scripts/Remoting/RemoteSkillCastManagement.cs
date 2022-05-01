using Assets.Scripts.Skills;
using Common;
using Common.GameDesign;
using Common.IoC;
using Common.Protocol.Map;
using Common.PublishSubscribe;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Remoting
{
	public class RemoteSkillCastManagement : MonoBehaviour
	{
		public List<RemoteSkillCastManagementEntry> Prefabs = new List<RemoteSkillCastManagementEntry>();

		private RemoteTargeting remoteTargeting;
		private IPubSub pubsub;

		public void OnEnable()
		{
			DILoader.Initialize();

			remoteTargeting = new RemoteTargeting(this.GetType().Name);

			pubsub = DI.Instance.Resolve<IPubSub>();
			pubsub.Subscribe<SkillCastMessage>(OnCastSkill, this.GetType().Name);
		}

		private void OnCastSkill(SkillCastMessage msg)
		{
			if (!remoteTargeting.CheckValidTargeting(msg.Target))
				return;

			GameObject go = GameObject.Instantiate(Prefabs.First(x => x.Type == msg.Type).Prefab);

			go.transform.position = remoteTargeting.GetTargetPosition(msg.Target);

			var fireball = go.GetComponent<Fireball>();
			if (fireball != null)
			{
				fireball.Caster = new Vector3(msg.Position.X, msg.Position.Y, 0);
				fireball.Target = remoteTargeting.GetTargetPosition(msg.Target);

				go.transform.position = fireball.Caster;
			}

			go.transform.SetParent(this.transform, true);

			var aoe = go.GetComponent<AoESkill>();
			if (aoe != null)
			{
				HandleAoE(msg, aoe);

			}
		}

		private static void HandleAoE(SkillCastMessage msg, AoESkill aoe)
		{
			if (!AoESkillsLoader.SkillCollisions.ContainsKey(msg.Type))
			{
				UnityEngine.Debug.LogError("Couldn't find Aoe Skills Loader Collision");
				GameObject.Destroy(aoe.gameObject);
				return;
			}

			var sc = AoESkillsLoader.SkillCollisions[msg.Type];
			aoe.WorldSize = new Vector2(sc.Size.X, sc.Size.Y);
			aoe.Type = msg.Type;
			aoe.RenderingDelay = 1.0f;
			aoe.IndicatorDelay = ((float)GameDesignConfiguration.SkillIndicatorDelay[msg.Type]) / 1000.0f;
		}
	}
}
