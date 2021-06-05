using Common.Protocol.Character;
using Common.Protocol.Map;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Character
{
	public class RemotePlayer
	{
		public GameObject GameObject;
		public SortedList<long, PlayerStateMessage> States;
		public CharacterInformation CharacterInformation = new CharacterInformation();

		internal void ShowCharacter(CharacterInformation characterInformation)
		{
			CharacterInformation = characterInformation;
			GameObject.GetComponent<CharacterStyleBehaviour>().SetStyle(characterInformation);
			GameObject.SetActive(true);
		}
	}
}
