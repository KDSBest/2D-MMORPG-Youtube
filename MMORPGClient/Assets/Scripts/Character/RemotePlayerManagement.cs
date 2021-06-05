using Assets.Scripts.PubSubEvents.CharacterClient;
using Common.IoC;
using Common.Protocol.Character;
using Common.Protocol.Map;
using Common.PublishSubscribe;
using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Character
{
	public class RemotePlayerManagement : MonoBehaviour
	{
		public GameObject RemotePlayerPrefab;
		private IPubSub pubsub;
		private Dictionary<string, RemotePlayer> remotePlayers;

		public void OnEnable()
		{
			DILoader.Initialize();

			remotePlayers = new Dictionary<string, RemotePlayer>();
		
			pubsub = DI.Instance.Resolve<IPubSub>();

			pubsub.Subscribe<PlayerStateMessage>(OnPlayerState, this.name);
			pubsub.Subscribe<RemoveStateMessage>(OnRemovePlayerState, this.name);
			pubsub.Subscribe<CharacterMessage>(OnCharacterMessage, this.name);
		}

		private void OnCharacterMessage(CharacterMessage data)
		{
			if (!remotePlayers.ContainsKey(data.Character.Name))
				return;

			remotePlayers[data.Character.Name].ShowCharacter(data.Character);
		}

		private void OnPlayerState(PlayerStateMessage data)
		{
			if(!remotePlayers.ContainsKey(data.Name))
			{
				GameObject newPlayer = GameObject.Instantiate(RemotePlayerPrefab);
				newPlayer.transform.SetParent(this.transform);
				newPlayer.SetActive(false);
				remotePlayers.Add(data.Name, new RemotePlayer()
				{
					GameObject = newPlayer,
					States = new SortedList<long, PlayerStateMessage>()
				});

				pubsub.Publish(new ReqCharacterStyle(data.Name));
			}

			// TODO: Interpolation and Jitter Protection
			if (!remotePlayers[data.Name].States.ContainsKey(data.ServerTime))
				remotePlayers[data.Name].States.Add(data.ServerTime, data);
			var lastState = remotePlayers[data.Name].States.Last().Value;
			var rPlayerGo = remotePlayers[data.Name].GameObject;
			rPlayerGo.transform.position = new Vector3(lastState.Position.X, lastState.Position.Y, 1);

			float xScaleAbs = Math.Abs(rPlayerGo.transform.localScale.x);
			if (lastState.IsLookingRight)
				rPlayerGo.transform.localScale = new Vector3(xScaleAbs, this.transform.localScale.y, this.transform.localScale.z);
			else
				rPlayerGo.transform.localScale = new Vector3(-xScaleAbs, this.transform.localScale.y, this.transform.localScale.z);
		}

		private void OnRemovePlayerState(RemoveStateMessage data)
		{
			if (!remotePlayers.ContainsKey(data.Name))
				return;

			if (remotePlayers[data.Name].States.Last().Key > data.ServerTime)
				return;

			GameObject.Destroy(remotePlayers[data.Name].GameObject);
			remotePlayers.Remove(data.Name);
		}
	}
}
