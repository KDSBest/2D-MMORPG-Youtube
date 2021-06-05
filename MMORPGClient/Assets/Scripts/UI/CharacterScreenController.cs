using Assets.Scripts.PubSubEvents.StartUI;
using Common.IoC;
using Common.Protocol.Character;
using Common.PublishSubscribe;
using TMPro;
using UnityEngine;
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
        public CharacterStyleBehaviour PlayerStylePicker;
        public CharacterStyleBehaviour Player;
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
            Eyes.maxValue = PlayerStylePicker.Eyes.Count - 1;
            Colors.maxValue = PlayerStylePicker.Colors.Count - 1;

            Background.SetActive(!data.Visible);
            PlayerStylePicker.gameObject.SetActive(data.Visible);
            CharacterScreen.SetActive(data.Visible);
        }

        private void OnCharacterMessage(CharacterMessage charMessage)
        {
            if (!CharacterScreen.activeSelf)
                return;

            Debug.Log($"Got Char Message with Name: {charMessage.Character.Name} and Token: {charMessage.Token}");
            Eyes.value = charMessage.Character.Eyes;
            Colors.value = charMessage.Character.Color;
            Name.text = charMessage.Character.Name;
            UpdateCharacter();
        }

        public void UpdateCharacter()
		{
            Player.SetStyle(new CharacterInformation()
            {
                Eyes = (byte)Eyes.value,
                Color = (byte)Colors.value,
                Name = Name.text
            });
            PlayerStylePicker.SetStyle(new CharacterInformation()
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