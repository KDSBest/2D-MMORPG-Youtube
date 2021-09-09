using Assets.Scripts.Character;
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
	public class InventoryScreenController : MonoBehaviour
    {
        private IPubSub pubsub;
        public GameObject InventoryScreen;
        public Transform InventorySlotParent;
        public GameObject InventorySlotPrefab;

        public TMP_Text Coins;

        public Sprite FlowerSprite;

        private Dictionary<string, InventorySlot> slots = new Dictionary<string, InventorySlot>();

        public void OnEnable()
        {
            DILoader.Initialize();
            pubsub = DI.Instance.Resolve<IPubSub>();

            pubsub.Subscribe<ToggleInventoryScreen>(OnToggleInventory, this.GetType().Name);
            pubsub.Subscribe<InventoryMessage>(OnInventory, this.GetType().Name);
        }

        public void ToggleInventoryScreen()
		{
            InventoryScreen.SetActive(!InventoryScreen.activeSelf);
        }

        private void OnToggleInventory(ToggleInventoryScreen toggle)
		{
            this.ToggleInventoryScreen();
        }

		private void OnInventory(InventoryMessage inv)
		{
            if (inv.Inventory.Items.ContainsKey(InventoryItemIds.Coins))
                Coins.text = inv.Inventory.Items[InventoryItemIds.Coins].ToString();

            foreach(var oldSlot in slots.Values)
			{
                oldSlot.Amount = 0;
			}

            foreach (var item in inv.Inventory.Items)
			{
                if (item.Key == InventoryItemIds.Coins)
                    continue;

                if(!slots.ContainsKey(item.Key))
				{
                    var go = GameObject.Instantiate(InventorySlotPrefab);
                    var slot = go.GetComponent<InventorySlot>();
                    slot.Image.sprite = FlowerSprite;
                    slot.transform.SetParent(InventorySlotParent);
                    slots.Add(item.Key, slot);
				}

                slots[item.Key].Amount = item.Value;
            }

            var toRemove = slots.Where(x => x.Value.Amount == 0).ToList();
            
            foreach (var remove in toRemove)
            {
                GameObject.Destroy(slots[remove.Key].gameObject);
                slots.Remove(remove.Key);
            }
        }

        public void OnDisable()
        {
            pubsub.Unsubscribe<ToggleInventoryScreen>(this.GetType().Name);
            pubsub.Unsubscribe<InventoryMessage>(this.GetType().Name);
        }
	}
}