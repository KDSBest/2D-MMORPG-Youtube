using Assets.Scripts;
using Assets.Scripts.PubSubEvents.StartUI;
using Common.IoC;
using Common.Protocol.Character;
using Common.PublishSubscribe;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class CharacterScreenController : MonoBehaviour
    {
        private IPubSub pubsub;
        public GameObject CharacterScreen;
        public TMP_InputField Name;
        public Slider Eyes;
        public Slider Colors;
        public CharacterStyleBehaviour Character;
        public GameObject Background;

        public void OnEnable()
        {
            DILoader.Initialize();
            pubsub = DI.Instance.Resolve<IPubSub>();

            pubsub.Subscribe<ControlCharacterScreen>(OnControlScreen, this.name);
            pubsub.Subscribe<CharacterMessage>(OnCharacterMessage, this.name);
        }

		public void OnDisable()
        {
            pubsub.Unsubscribe<ControlCharacterScreen>(this.name);
            pubsub.Unsubscribe<CharacterMessage>(this.name);
        }

        public void OnControlScreen(ControlCharacterScreen data)
        {
            Eyes.maxValue = Character.Eyes.Count - 1;
            Colors.maxValue = Character.Colors.Count - 1;

            Background.SetActive(!data.Visible);
            Character.gameObject.SetActive(data.Visible);
            CharacterScreen.SetActive(data.Visible);
        }

        private void OnCharacterMessage(CharacterMessage charMessage)
        {
            Debug.Log($"Got Char Message with Name: {charMessage.Character.Name} and Token: {charMessage.Token}");
            Eyes.value = charMessage.Character.Eyes;
            Colors.value = charMessage.Character.Color;
            Name.text = charMessage.Character.Name;
            UpdateCharacter();
        }

        public void UpdateCharacter()
		{
            Character.SetStyle(new CharacterInformation()
            {
                Eyes = (byte)Eyes.value,
                Color = (byte)Colors.value,
                Name = Name.text
            });
        }

        public void SendCharacterCreation()
		{
            pubsub.Publish(new CharacterInformation()
            {
                Eyes = (byte)Eyes.value,
                Color = (byte)Colors.value,
                Name = Name.text
            });
		}
	}
}