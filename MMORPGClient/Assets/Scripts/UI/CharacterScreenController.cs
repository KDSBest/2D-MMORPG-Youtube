using Assets.Scripts.Character;
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
        public CharacterBehaviour Player;
        public GameObject Background;

        public void OnEnable()
        {
            DILoader.Initialize();
            pubsub = DI.Instance.Resolve<IPubSub>();

            pubsub.Subscribe<ControlCharacterScreen>(OnControlScreen, this.GetType().Name);
            pubsub.Subscribe<CharacterMessage>(OnCharacterMessage, this.GetType().Name);
        }

		public void OnDisable()
        {
            pubsub.Unsubscribe<ControlCharacterScreen>(this.GetType().Name);
            pubsub.Unsubscribe<CharacterMessage>(this.GetType().Name);
        }

        public void OnControlScreen(ControlCharacterScreen data)
        {
            Eyes.maxValue = Player.Eyes.Count - 1;
            Colors.maxValue = Player.Colors.Count - 1;

            Background.SetActive(!data.Visible);
            CharacterScreen.SetActive(data.Visible);
        }

        private void OnCharacterMessage(CharacterMessage charMessage)
        {
            if (!CharacterScreen.activeSelf)
                return;

            Eyes.value = charMessage.Character.Eyes;
            Colors.value = charMessage.Character.Color;
            Name.text = charMessage.Character.Name;
            UpdateCharacter();
        }

        public void UpdateCharacter()
		{
            Player.UpdateCharInfo(new CharacterInformation()
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