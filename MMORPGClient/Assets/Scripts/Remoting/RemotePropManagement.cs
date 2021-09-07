using Assets.Scripts.Prop;
using Assets.Scripts.PubSubEvents.CharacterClient;
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

	public class RemotePropManagement : MonoBehaviour
	{
		public GameObject RemotePropPrefab;
		private IPubSub pubsub;
		private Dictionary<string, PropBehaviour> remoteProps;

		public int PlayerCount
		{
			get
			{
				return remoteProps.Count;
			}
		}

		public void OnEnable()
		{
			DILoader.Initialize();

			remoteProps = new Dictionary<string, PropBehaviour>();

			pubsub = DI.Instance.Resolve<IPubSub>();
			pubsub.Subscribe<PropStateMessage>(OnPropState, this.GetType().Name);
		}

		private void OnPropState(PropStateMessage state)
		{
			if (!remoteProps.ContainsKey(state.Name))
			{
				GameObject go = GameObject.Instantiate(RemotePropPrefab);
				go.transform.SetParent(this.transform);
				go.name = state.Name;

				var newProp = go.GetComponent<PropBehaviour>();
				newProp.Name = state.Name;
				
				remoteProps.Add(state.Name, newProp);
			}

			var prop = remoteProps[state.Name];
			prop.Health = state.Health;
			prop.MaxHealth = state.MaxHealth;
			prop.gameObject.SetActive(prop.Health > 0);
			prop.transform.position = new Vector3(state.Position.X, state.Position.Y, 0);
		}

	}
}
