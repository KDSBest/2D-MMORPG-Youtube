using Common.Protocol.Character;
using Common.Protocol.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Character
{
	public class RemotePlayer
	{
		public GameObject GameObject;
		public SortedList<long, PlayerStateMessage> States;
		public CharacterInformation CharacterInformation = new CharacterInformation();
		private CharacterStyleBehaviour characterStyle;
		private RemotePlayerRenderer playerRenderer;

		public void Initialize()
		{
			characterStyle = GameObject.GetComponent<CharacterStyleBehaviour>();
			playerRenderer = GameObject.GetComponent<RemotePlayerRenderer>();
		}

		public void Update()
		{
			var lastState = States.Last().Value;
			GameObject.transform.position = new Vector3(lastState.Position.X, lastState.Position.Y, 1);

			playerRenderer.SetLooking(lastState.IsLookingRight);
		}

		public void SetStyle(CharacterInformation characterInformation)
		{
			CharacterInformation = characterInformation;
			characterStyle.SetStyle(characterInformation);
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
