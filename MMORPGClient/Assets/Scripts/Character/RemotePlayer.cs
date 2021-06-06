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
		public float LerpSpeed = 0.5f;
		private CharacterStyleBehaviour characterStyle;
		private RemotePlayerRenderer playerRenderer;
		private Animator animator;

		public void Initialize()
		{
			characterStyle = GameObject.GetComponent<CharacterStyleBehaviour>();
			playerRenderer = GameObject.GetComponent<RemotePlayerRenderer>();
			animator = GameObject.GetComponent<Animator>();
		}

		public void Update()
		{
			var lastState = States.Last().Value;

			GameObject.transform.position = Vector3.Lerp(GameObject.transform.position, new Vector3(lastState.Position.X, lastState.Position.Y, 0), LerpSpeed);
			animator.SetInteger(Constants.AnimationStateName, lastState.Animation);
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
