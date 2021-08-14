using Assets.Scripts.Character;
using Assets.Scripts.PubSubEvents.StartUI;
using Common.IoC;
using Common.Protocol.Character;
using Common.Protocol.Inventory;
using Common.PublishSubscribe;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
	public class HUDScreenController : MonoBehaviour
    {
        private IPubSub pubsub;
        public TMP_Text Coins;

        public void OnEnable()
        {
            DILoader.Initialize();
            pubsub = DI.Instance.Resolve<IPubSub>();

            pubsub.Subscribe<InventoryMessage>(OnInventory, this.name);
        }

		private void OnInventory(InventoryMessage inv)
		{
            if (inv.Inventory.Items.ContainsKey("Gold"))
                Coins.text = inv.Inventory.Items["Gold"].ToString();
		}

		public void OnDisable()
        {
            pubsub.Unsubscribe<InventoryMessage>(this.name);
        }
	}
}