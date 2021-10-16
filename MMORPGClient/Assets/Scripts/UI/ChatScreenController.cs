using Assets.Scripts.Character;
using Assets.Scripts.PubSubEvents.ChatClient;
using Assets.Scripts.PubSubEvents.StartUI;
using Assets.Scripts.UI.SubScreen;
using Common.IoC;
using Common.Protocol.Chat;
using Common.PublishSubscribe;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
	public class ChatScreenController : MonoBehaviour
	{
		private IPubSub pubsub;
		public GameObject ChatScreen;
		public GameObject ChatMessagePrefab;
		public Transform ChatMessageParent;
		public TMP_InputField ChatMessageInput;
		public ScrollRect ScrollRect;
		public GameObject Background;

		public void Awake()
		{
			ChatMessageInput.onEndEdit.AddListener(new UnityAction<string>(OnChatInputEnd));
			ChatMessageInput.onSelect.AddListener(new UnityAction<string>(OnSelect));
			ChatMessageInput.onDeselect.AddListener(new UnityAction<string>(OnDeselect));
		}

		private void OnDeselect(string ev)
		{
			pubsub.Publish<PlayerControlEnable>(new PlayerControlEnable()
			{
				Enabled = true
			});
		}

		private void OnChatInputEnd(string ev)
		{
			SendChatMessage();
		}

		private void OnSelect(string ev)
		{
			pubsub.Publish<PlayerControlEnable>(new PlayerControlEnable()
			{
				Enabled = false
			});
		}

		public void OnEnable()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();

			pubsub.Subscribe<ControlChatScreen>(OnControlScreen, this.GetType().Name);
			pubsub.Subscribe<ChatMessage>(OnChatMessage, this.GetType().Name);
		}

		private void OnChatMessage(ChatMessage chatMessage)
		{
			GameObject newMessageGo = GameObject.Instantiate(ChatMessagePrefab);
			newMessageGo.transform.SetParent(ChatMessageParent);
			newMessageGo.GetComponent<ItemLinkDetection>().SetText($"{chatMessage.Sender}: {chatMessage.Message}");

			bool isScrolledToBottom = ScrollRect.normalizedPosition.y < 0.0001f;
			if (isScrolledToBottom)
			{
				// First Unity Layout has to recalculate the new size then we can scroll
				StartCoroutine(AutoScrollChat());
			}
		}

		private IEnumerator AutoScrollChat()
		{
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			ScrollRect.normalizedPosition = new Vector2(ScrollRect.normalizedPosition.x, 0);
		}

		public void SendChatMessage()
		{
			string message = ChatMessageInput.text;
			if (!string.IsNullOrEmpty(message))
			{
				pubsub.Publish(new SendChatMessage()
				{
					Message = message
				});
				ChatMessageInput.text = string.Empty;
				ChatMessageInput.Select();
				ChatMessageInput.ActivateInputField();
			}
		}

		public void OnDisable()
		{
			pubsub.Unsubscribe<ControlChatScreen>(this.GetType().Name);
			pubsub.Unsubscribe<ChatMessage>(this.GetType().Name);
		}

		public void OnControlScreen(ControlChatScreen data)
		{
			ChatScreen.SetActive(data.Visible);
			Background.SetActive(!data.Visible);
		}
	}
}