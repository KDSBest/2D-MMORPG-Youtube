using Assets.Scripts.Character;
using Common.Protocol.Character;
using Common.Protocol.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Remoting
{
	public class RemotePlayer
	{
		public GameObject GameObject;
		public SortedList<long, PlayerStateMessage> States;
		public CharacterInformation CharacterInformation = new CharacterInformation();
		public float LerpSpeed = 0.5f;
		private CharacterBehaviour characterStyle;
		private RemotePlayerRenderer playerRenderer;

		public void Initialize()
		{
			characterStyle = GameObject.GetComponent<CharacterBehaviour>();
			playerRenderer = GameObject.GetComponent<RemotePlayerRenderer>();
		}

		public void Update()
		{
			var lastState = States.Last().Value;

			GameObject.transform.position = Vector3.Lerp(GameObject.transform.position, new Vector3(lastState.Position.X, lastState.Position.Y, 0), LerpSpeed);
			playerRenderer.SetAnimation(lastState.Animation);
			playerRenderer.SetLooking(lastState.IsLookingRight);
		}

		public void UpdateCharInfo(CharacterInformation characterInformation)
		{
			CharacterInformation = characterInformation;
			characterStyle.UpdateCharInfo(characterInformation);
		}

		public void ShowCharacter()
		{
			GameObject.SetActive(true);
		}

		public void HideCharacter()
		{
			GameObject.SetActive(false);
		}
	}
}
