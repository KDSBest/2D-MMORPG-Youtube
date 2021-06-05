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
	public class RemotePlayerManagement : MonoBehaviour
	{
		public GameObject RemotePlayerPrefab;
		private IPubSub pubsub;
		private Dictionary<string, RemotePlayer> remotePlayers;
		private ICurrentContext context;

		public void OnEnable()
		{
			DILoader.Initialize();

			remotePlayers = new Dictionary<string, RemotePlayer>();
		
			context = DI.Instance.Resolve<ICurrentContext>();
			pubsub = DI.Instance.Resolve<IPubSub>();

			pubsub.Subscribe<PlayerStateMessage>(OnPlayerState, this.name);
			pubsub.Subscribe<RemoveStateMessage>(OnRemovePlayerState, this.name);
			pubsub.Subscribe<CharacterMessage>(OnCharacterMessage, this.name);
		}

		private void OnCharacterMessage(CharacterMessage data)
		{
			if (!remotePlayers.ContainsKey(data.Character.Name))
				return;

			var player = remotePlayers[data.Character.Name];
			player.SetStyle(data.Character);

			if(data.Character.Name != context.Name)
			{
				player.ShowCharacter();
			}
		}

		private void OnPlayerState(PlayerStateMessage data)
		{
			if(!remotePlayers.ContainsKey(data.Name))
			{
				GameObject newPlayer = GameObject.Instantiate(RemotePlayerPrefab);
				newPlayer.transform.SetParent(this.transform);

				var player = new RemotePlayer()
				{
					GameObject = newPlayer,
					States = new SortedList<long, PlayerStateMessage>()
				};
				player.HideCharacter();

				remotePlayers.Add(data.Name, player);

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
