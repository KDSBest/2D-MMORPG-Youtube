using Assets.Scripts.UI.SubScreen;
using Common.IoC;
using Common.Protocol.Inventory;
using Common.Protocol.PlayerEvent;
using Common.PublishSubscribe;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
	public class InventoryChangeScreenController : MonoBehaviour
    {
        private IPubSub pubsub;
        public Transform ParentObject;
        public GameObject Prefab;
        public Sprite CoinImage;
        public Sprite FlowerImage;

        public void OnEnable()
        {
            DILoader.Initialize();
            pubsub = DI.Instance.Resolve<IPubSub>();
            pubsub.Subscribe<PlayerEventMessage>(OnPlayerEvent, this.GetType().Name);
        }

		private void OnPlayerEvent(PlayerEventMessage ev)
		{
            foreach(var add in ev.Add)
			{
                CreateEventEntry($"+ {add.Value}", add.Key);
			}

            foreach (var remove in ev.Remove)
            {
                CreateEventEntry($"- {remove.Value}", remove.Key);
            }
        }

		private void CreateEventEntry(string displayValue, string type)
		{
            var entryGo = GameObject.Instantiate(Prefab);
            entryGo.transform.SetParent(ParentObject);
            var invChange = entryGo.GetComponent<InventoryChangeEntry>();
            invChange.ValueText.text = displayValue;

            if (type == "Flower")
                invChange.Image.sprite = FlowerImage;
            else
                invChange.Image.sprite = CoinImage;
            
        }

		public void OnDisable()
        {
            pubsub.Unsubscribe<PlayerEventMessage>(this.GetType().Name);
        }
	}
}