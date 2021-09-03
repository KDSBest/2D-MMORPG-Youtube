using Assets.Scripts.PubSubEvents.CharacterClient;
using Common.IoC;
using Common.Protocol.Character;
using Common.Protocol.Map;
using Common.PublishSubscribe;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Character
{

	public class RemotePropManagement : MonoBehaviour
	{
		public GameObject RemotePropPrefab;
		private IPubSub pubsub;
		private Dictionary<string, GameObject> remoteProps;

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

			remoteProps = new Dictionary<string, GameObject>();

			pubsub = DI.Instance.Resolve<IPubSub>();
			pubsub.Subscribe<PropStateMessage>(OnPropState, this.GetType().Name);
		}

		private void OnPropState(PropStateMessage state)
		{
			if (!remoteProps.ContainsKey(state.Name))
			{
				GameObject newProp = GameObject.Instantiate(RemotePropPrefab);
				newProp.transform.SetParent(this.transform);

				remoteProps.Add(state.Name, newProp);
			}

			remoteProps[state.Name].transform.position = new Vector3(state.Position.X, state.Position.Y, 0);
		}

	}
}
