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
        public GameObject HUD;
        public TMP_Text Coins;

        public void OnEnable()
        {
            DILoader.Initialize();
            pubsub = DI.Instance.Resolve<IPubSub>();

            pubsub.Subscribe<InventoryMessage>(OnInventory, this.GetType().Name);
        }

		private void OnInventory(InventoryMessage inv)
		{
            HUD.SetActive(true);

            if (inv.Inventory.Items.ContainsKey(InventoryItemIds.Coins))
                Coins.text = inv.Inventory.Items[InventoryItemIds.Coins].ToString();
            else
                Coins.text = "0";
		}

		public void OnDisable()
        {
            pubsub.Unsubscribe<InventoryMessage>(this.GetType().Name);
        }
	}
}