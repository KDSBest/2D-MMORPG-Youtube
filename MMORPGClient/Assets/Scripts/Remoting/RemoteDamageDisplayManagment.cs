using Assets.Scripts.Prop;
using Assets.Scripts.PubSubEvents.CharacterClient;
using Assets.Scripts.Skills;
using Common.GameDesign;
using Common.IoC;
using Common.Protocol.Character;
using Common.Protocol.Combat;
using Common.Protocol.Map;
using Common.PublishSubscribe;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Remoting
{
	public class RemoteDamageDisplayManagment : MonoBehaviour
	{
		public GameObject DamageNumberPrefab;

		private IPubSub pubsub;
		private Dictionary<string, Vector2> targetLocations = new Dictionary<string, Vector2>();


		public void OnEnable()
		{
			DILoader.Initialize();

			pubsub = DI.Instance.Resolve<IPubSub>();
			pubsub.Subscribe<DamageMessage>(OnDamage, this.GetType().Name);
			pubsub.Subscribe<PropStateMessage>(OnPropState, this.GetType().Name);
		}

		private void OnDamage(DamageMessage msg)
		{
			if (!CheckValidTargeting(msg))
				return;

			var go = GameObject.Instantiate(DamageNumberPrefab);
			go.transform.position = GetTargetPosition(msg);
			go.transform.SetParent(this.transform, true);
			var dmgDisplay = go.GetComponent<DamageDisplay>();
			dmgDisplay.SetDamage(msg.Damage);
		}

		private bool CheckValidTargeting(DamageMessage msg)
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

		private Vector3 GetTargetPosition(DamageMessage msg)
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

		private void OnPropState(PropStateMessage state)
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
