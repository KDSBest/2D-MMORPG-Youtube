using Assets.Scripts.Prop;
using Assets.Scripts.PubSubEvents.CharacterClient;
using Common.IoC;
using Common.Protocol.Character;
using Common.Protocol.Map;
using Common.PublishSubscribe;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Remoting
{
	public class RemoteEnemyManagement : MonoBehaviour
	{
		public List<RemoteEnemyManagementEntry> Config = new List<RemoteEnemyManagementEntry>();
		private IPubSub pubsub;
		private Dictionary<string, EnemyBehaviour> remoteProps;

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

			remoteProps = new Dictionary<string, EnemyBehaviour>();

			pubsub = DI.Instance.Resolve<IPubSub>();
			pubsub.Subscribe<EnemyStateMessage>(OnEnemyState, this.GetType().Name);
		}

		private void OnEnemyState(EnemyStateMessage state)
		{
			if (!remoteProps.ContainsKey(state.Name))
			{
				GameObject go = GameObject.Instantiate(Config.First(x => x.Type == state.Type).Prefab);
				go.transform.SetParent(this.transform);
				go.name = state.Name;

				var newProp = go.GetComponent<EnemyBehaviour>();
				newProp.Name = state.Name;
				
				remoteProps.Add(state.Name, newProp);
			}

			var prop = remoteProps[state.Name];
			prop.HP = state.Stats.HP;
			prop.MaxHP = state.Stats.MaxHP;
			prop.gameObject.SetActive(prop.HP > 0);
			prop.transform.position = new Vector3(state.Position.X, state.Position.Y, 0);
		}

	}
}
