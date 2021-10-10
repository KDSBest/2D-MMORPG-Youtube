using Assets.Scripts.Character;
using Assets.Scripts.PubSubEvents.StartUI;
using Assets.Scripts.UI.SubScreen;
using Common.IoC;
using Common.Protocol.Character;
using Common.Protocol.Combat;
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

        public AutoHideInfoController ExpGain;
        public ExpBarController ExpController;
        public TMP_Text Level;
        private ICurrentContext context;

        public void OnEnable()
        {
            DILoader.Initialize();
            pubsub = DI.Instance.Resolve<IPubSub>();

            pubsub.Subscribe<InventoryMessage>(OnInventory, this.GetType().Name);
            pubsub.Subscribe<CharacterMessage>(OnCharacterMessage, this.GetType().Name);
            pubsub.Subscribe<ExpMessage>(OnExpMessage, this.GetType().Name);

            context = DI.Instance.Resolve<ICurrentContext>();
        }

		private void OnExpMessage(ExpMessage msg)
		{
            context.Character.Experience += msg.ExpGain;
            ExpGain.Show($"+ {msg.ExpGain}", 5);

            ExpController.UpdateExp(context.Character.Experience, context.Character.Level);
        }

		private void OnCharacterMessage(CharacterMessage msg)
		{
            if(context.Character != null && msg.Character.Name == context.Character.Name)
			{
                Level.text = msg.Character.Level.ToString();
                ExpController.UpdateExp(msg.Character.Experience, msg.Character.Level);

                // TODO: Play level up animation
            }
        }

		private void OnInventory(InventoryMessage inv)
		{
            HUD.SetActive(true);

            Level.text = this.context.Character.Level.ToString();

            if (inv.Inventory.Items.ContainsKey(InventoryItemIds.Coins))
                Coins.text = inv.Inventory.Items[InventoryItemIds.Coins].ToString();
            else
                Coins.text = "0";
		}

		public void OnDisable()
        {
            pubsub.Unsubscribe<InventoryMessage>(this.GetType().Name);
            pubsub.Unsubscribe<CharacterMessage>(this.GetType().Name);
            pubsub.Unsubscribe<ExpMessage>(this.GetType().Name);
        }
    }
}