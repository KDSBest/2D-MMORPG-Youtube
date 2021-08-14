using Assets.Scripts.Character;
using Assets.Scripts.PubSubEvents.StartUI;
using Common.IoC;
using Common.Protocol.Character;
using Common.Protocol.Inventory;
using Common.Protocol.PlayerEvent;
using Common.PublishSubscribe;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
	public class DailyLoginScreenController : MonoBehaviour
    {
        private IPubSub pubsub;
        public GameObject DailyLogin;

        public void OnEnable()
        {
            DILoader.Initialize();
            pubsub = DI.Instance.Resolve<IPubSub>();

            pubsub.Subscribe<PlayerEventMessage>(OnPlayerEvent, this.name);
        }

		private void OnPlayerEvent(PlayerEventMessage ev)
		{
            if(ev.Type == PlayerEventType.DailyLogin)
			{
                DailyLogin.SetActive(true);
			}
		}

		public void OnDisable()
        {
            pubsub.Unsubscribe<InventoryMessage>(this.name);
        }
	}
}