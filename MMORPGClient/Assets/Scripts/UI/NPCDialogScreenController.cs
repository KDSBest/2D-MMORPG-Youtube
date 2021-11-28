using Assets.Scripts.Character;
using Assets.Scripts.PubSubEvents.Dialog;
using Assets.Scripts.PubSubEvents.StartUI;
using Assets.Scripts.UI.SubScreen;
using Common.IoC;
using Common.Protocol.Character;
using Common.Protocol.Inventory;
using Common.PublishSubscribe;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
	public class NPCDialogScreenController : MonoBehaviour
    {
        private IPubSub pubsub;
        public GameObject DialogScreen;

        public TMP_Text Name;

        public TMP_Text Text;

        public Button[] DialogOptions;
        public TMP_Text[] DialogOptionsText;

        public void OnEnable()
        {
            DILoader.Initialize();
            pubsub = DI.Instance.Resolve<IPubSub>();

            pubsub.Subscribe<ShowDialog>(OnShowDialog, this.GetType().Name);
        }

		private void OnShowDialog(ShowDialog data)
		{
            Name.text = data.Name;
            Text.text = data.Text;

            foreach(var option in DialogOptions)
			{
                option.gameObject.SetActive(false);
			}

            for(int i = 0; i < data.DialogOptions.Length && i < DialogOptions.Length && i < DialogOptionsText.Length; i++)
			{
                DialogOptionsText[i].text = data.DialogOptions[i];
                DialogOptions[i].gameObject.SetActive(true);
			}

            DialogScreen.SetActive(true);
        }

        public void OnDialogOption(int option)
		{
            DialogScreen.SetActive(false);
            pubsub.Publish(new SelectDialogOption()
            {
                OptionText = DialogOptionsText[option].text,
                Option = option
            });
		}

        public void OnDisable()
        {
            pubsub.Unsubscribe<ShowDialog>(this.GetType().Name);
        }
	}
}