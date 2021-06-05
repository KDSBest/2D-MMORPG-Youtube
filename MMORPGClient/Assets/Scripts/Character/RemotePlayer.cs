using Common.Protocol.Character;
using Common.Protocol.Map;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Character
{
	public class RemotePlayer
	{
		public GameObject GameObject;
		public SortedList<long, PlayerStateMessage> States;
		public CharacterInformation CharacterInformation = new CharacterInformation();

		public void SetStyle(CharacterInformation characterInformation)
		{
			CharacterInformation = characterInformation;
			GameObject.GetComponent<CharacterStyleBehaviour>().SetStyle(characterInformation);
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
