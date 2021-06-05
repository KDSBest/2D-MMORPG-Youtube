using Assets.Scripts.PubSubEvents.ChatClient;
using Assets.Scripts.PubSubEvents.StartUI;
using Common.IoC;
using Common.Protocol.Chat;
using Common.PublishSubscribe;
using System.Collections;
using TMPro;
using UnityEngine;
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

		public void OnEnable()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();

			pubsub.Subscribe<ControlChatScreen>(OnControlScreen, this.name);
			pubsub.Subscribe<ChatMessage>(OnChatMessage, this.name);
		}

		private void OnChatMessage(ChatMessage chatMessage)
		{
			GameObject newMessageGo = GameObject.Instantiate(ChatMessagePrefab);
			newMessageGo.GetComponent<TMP_Text>().text = $"{chatMessage.Sender}: {chatMessage.Message}";
			newMessageGo.transform.SetParent(ChatMessageParent);

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
			pubsub.Unsubscribe<ControlChatScreen>(this.name);
			pubsub.Unsubscribe<ChatMessage>(this.name);
		}

		public void OnControlScreen(ControlChatScreen data)
		{
			ChatScreen.SetActive(data.Visible);
			Background.SetActive(!data.Visible);
		}
	}
}