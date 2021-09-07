using Assets.Scripts.Character;
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

	public class RemotePlayerManagement : MonoBehaviour
	{
		public GameObject RemotePlayerPrefab;
		private IPubSub pubsub;
		private Dictionary<string, RemotePlayer> remotePlayers;
		private ICurrentContext context;
		private RemotePlayerRemovedTracking removedTracking = new RemotePlayerRemovedTracking();

		public int PlayerCount
		{
			get
			{
				return remotePlayers.Count;
			}
		}

		public void OnEnable()
		{
			DILoader.Initialize();

			remotePlayers = new Dictionary<string, RemotePlayer>();

			context = DI.Instance.Resolve<ICurrentContext>();
			pubsub = DI.Instance.Resolve<IPubSub>();

			pubsub.Subscribe<PlayerStateMessage>(OnPlayerState, this.GetType().Name);
			pubsub.Subscribe<RemoveStateMessage>(OnRemovePlayerState, this.GetType().Name);
			pubsub.Subscribe<CharacterMessage>(OnCharacterMessage, this.GetType().Name);
		}

		private void OnCharacterMessage(CharacterMessage charDisplayData)
		{
			if (!remotePlayers.ContainsKey(charDisplayData.Character.Name))
				return;

			var player = remotePlayers[charDisplayData.Character.Name];
			player.SetStyle(charDisplayData.Character);

			if (charDisplayData.Character.Name != context.Name)
			{
				player.ShowCharacter();
			}
		}

		private void OnPlayerState(PlayerStateMessage state)
		{
			// ignore new player state if remove state is newer
			if (removedTracking.GetPlayerRemovedServerTime(state.Name) >= state.ServerTime)
				return;

			if (!remotePlayers.ContainsKey(state.Name))
			{
				GameObject newPlayer = GameObject.Instantiate(RemotePlayerPrefab);
				newPlayer.transform.SetParent(this.transform);

				var player = new RemotePlayer()
				{
					GameObject = newPlayer,
					States = new SortedList<long, PlayerStateMessage>()
				};

				player.Initialize();
				player.HideCharacter();

				remotePlayers.Add(state.Name, player);

				pubsub.Publish(new ReqCharacterStyle(state.Name));
			}

			// TODO: Interpolation and Jitter Protection
			if (!remotePlayers[state.Name].States.ContainsKey(state.ServerTime))
				remotePlayers[state.Name].States.Add(state.ServerTime, state);

			remotePlayers[state.Name].Update();
		}

		private void OnRemovePlayerState(RemoveStateMessage removeState)
		{
			removedTracking.UpdateRemovedPlayerStateTime(removeState);

			if (!remotePlayers.ContainsKey(removeState.Name))
				return;

			if (remotePlayers[removeState.Name].States.Last().Key > removeState.ServerTime)
				return;

			GameObject.Destroy(remotePlayers[removeState.Name].GameObject);
			remotePlayers.Remove(removeState.Name);
		}
	}
}
