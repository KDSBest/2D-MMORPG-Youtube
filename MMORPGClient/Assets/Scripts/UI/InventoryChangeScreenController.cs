using Assets.Scripts.UI.SubScreen;
using Common.IoC;
using Common.Protocol.Inventory;
using Common.Protocol.PlayerEvent;
using Common.PublishSubscribe;
using UnityEngine;

namespace Assets.Scripts.UI
{
	public class InventoryChangeScreenController : MonoBehaviour
    {
        private IPubSub pubsub;
        public Transform ParentObject;
        public GameObject Prefab;

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
                CreateEventEntry($"+ {add.Value}");
			}

            foreach (var remove in ev.Remove)
            {
                CreateEventEntry($"- {remove.Value}");
            }

            if(ev.Add.Count > 0 || ev.Remove.Count > 0)
			{
                pubsub.Publish<RequestInventoryMessage>(new RequestInventoryMessage());
			}
        }

		private void CreateEventEntry(string displayValue)
		{
            var entryGo = GameObject.Instantiate(Prefab);
            entryGo.transform.SetParent(ParentObject);
            entryGo.GetComponent<InventoryChangeEntry>().ValueText.text = displayValue;
        }

		public void OnDisable()
        {
            pubsub.Unsubscribe<PlayerEventMessage>(this.GetType().Name);
        }
	}
}