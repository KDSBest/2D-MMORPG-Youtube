using Assets.Scripts.Character;
using Assets.Scripts.PubSubEvents.StartUI;
using Assets.Scripts.UI.SubScreen;
using Common;
using Common.IoC;
using Common.Protocol.Inventory;
using Common.Protocol.Quest;
using Common.PublishSubscribe;
using UnityEngine;

namespace Assets.Scripts.UI
{
	public class QuestScreenController : MonoBehaviour
	{
		private IPubSub pubsub;
		private ICurrentContext context;
		public GameObject QuestScreen;
		public GameObject QuestPrefab;
		public Transform QuestParent;

		public void OnEnable()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();

			context = DI.Instance.Resolve<ICurrentContext>();
			pubsub.Subscribe<ControlQuestScreen>(OnControlScreen, this.GetType().Name);
			pubsub.Subscribe<ResponseQuestTrackingMessage>(OnQuestTracking, this.GetType().Name);
			pubsub.Subscribe<InventoryMessage>(OnInventory, this.GetType().Name);
		}

		public void OnDisable()
		{
			pubsub.Unsubscribe<InventoryMessage>(this.GetType().Name);
			pubsub.Unsubscribe<ControlQuestScreen>(this.GetType().Name);
			pubsub.Unsubscribe<ResponseQuestTrackingMessage>(this.GetType().Name);
		}

		public void OnControlScreen(ControlQuestScreen data)
		{
			QuestScreen.SetActive(data.Visible);
			pubsub.Publish<RequestQuestTracking>(new RequestQuestTracking());
		}

		private void OnInventory(InventoryMessage data)
		{
			if (context.QuestTracking != null)
			{
				context.QuestTracking.Inventory = data.Inventory;
			}

			UpdateQuests();
		}

		private void OnQuestTracking(ResponseQuestTrackingMessage data)
		{
			data.QuestTracking.Inventory = context.Inventory;
			context.QuestTracking = data.QuestTracking;

			UpdateQuests();
		}

		private void UpdateQuests()
		{
			foreach (Transform child in QuestParent)
			{
				GameObject.Destroy(child.gameObject);
			}

			if (context.QuestTracking == null || context.QuestTracking.Inventory == null)
			{
				return;
			}

			foreach (var entry in context.QuestTracking.AcceptedQuests)
			{
				GameObject questGo = GameObject.Instantiate(QuestPrefab);
				questGo.transform.SetParent(QuestParent);
				var questEntry = questGo.GetComponent<QuestEntry>();
				string qName = entry;
				var quest = QuestLoader.Quests[qName];
				questEntry.Abbandon.onClick.AddListener(() =>
				{
					pubsub.Publish<AbbandonQuestMessage>(new AbbandonQuestMessage()
					{
						QuestName = qName
					});
				});
				questEntry.QuestName.text = quest.Name;
				questEntry.QuestText.SetText(quest.Task.GetDisplay(qName, context.QuestTracking));
			}
		}
	}
}