using Assets.Scripts.Prop;
using Assets.Scripts.PubSubEvents.CharacterClient;
using Assets.Scripts.Skills;
using Common.GameDesign;
using Common.IoC;
using Common.Protocol.Character;
using Common.Protocol.Map;
using Common.PublishSubscribe;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Remoting
{
	public class RemoteSkillCastManagement : MonoBehaviour
	{
		public GameObject RemoteFireballPrefab;
		public GameObject RemoteLightningBoltPrefab;

		private IPubSub pubsub;
		private Dictionary<string, Vector2> targetLocations = new Dictionary<string, Vector2>();


		public void OnEnable()
		{
			DILoader.Initialize();

			pubsub = DI.Instance.Resolve<IPubSub>();
			pubsub.Subscribe<SkillCastMessage>(OnCastSkill, this.GetType().Name);
			pubsub.Subscribe<EnemyStateMessage>(OnPropState, this.GetType().Name);
		}

		private void OnCastSkill(SkillCastMessage msg)
		{
			if (!CheckValidTargeting(msg))
				return;

			GameObject go;

			switch(msg.Type)
			{
				case SkillCastType.Fireball:
					go = GameObject.Instantiate(RemoteFireballPrefab);

					var fireball = go.GetComponent<Fireball>();
					fireball.Caster = new Vector3(msg.Position.X, msg.Position.Y, 0);
					fireball.Target = GetTargetPosition(msg);

					go.transform.position = fireball.Caster;
					break;
				case SkillCastType.LightningBolt:
					go = GameObject.Instantiate(RemoteLightningBoltPrefab);

					go.transform.position = GetTargetPosition(msg);
					break;
				default:
					return;
			}

			go.transform.SetParent(this.transform, true);
		}

		private bool CheckValidTargeting(SkillCastMessage msg)
		{
			switch (msg.Target.TargetType)
			{
				case SkillCastTargetType.SingleTarget:
					return targetLocations.ContainsKey(msg.Target.TargetName);
				case SkillCastTargetType.Position:
					return true;
			}

			return false;
		}

		private Vector3 GetTargetPosition(SkillCastMessage msg)
		{
			switch (msg.Target.TargetType)
			{
				case SkillCastTargetType.SingleTarget:
					if (!targetLocations.ContainsKey(msg.Target.TargetName))
					{
						return Vector3.zero;
					}

					return targetLocations[msg.Target.TargetName];
				case SkillCastTargetType.Position:
					return new Vector3(msg.Target.TargetPosition.X, msg.Target.TargetPosition.Y, 0);
			}

			return Vector3.zero;
		}

		private void OnPropState(EnemyStateMessage state)
		{
			if (!targetLocations.ContainsKey(state.Name))
			{
				targetLocations.Add(state.Name, new Vector2(state.Position.X, state.Position.Y));
			}
			else
			{
				targetLocations[state.Name] = new Vector2(state.Position.X, state.Position.Y);
			}
		}
	}
}
