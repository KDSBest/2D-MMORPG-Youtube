using Assets.Scripts.Character;
using Assets.Scripts.PubSubEvents.ChatClient;
using Common.Client.Interfaces;
using Common.IoC;
using Common.Protocol.Quest;
using Common.PublishSubscribe;
using System;
using System.Threading.Tasks;

namespace Assets.Scripts.ClientWrappers
{
	public class QuestTrackingClientWrapper : IQuestTrackingClientWrapper
	{
		public IQuestTrackingClient client;

		public bool IsInitialized { get { return client.IsConnected; } }

		private IPubSub pubsub;

		public QuestTrackingClientWrapper()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
			client = DI.Instance.Resolve<IQuestTrackingClient>();
			pubsub.Subscribe<RequestQuestTracking>(OnRequestQuestTracking, this.GetType().Name);
			pubsub.Subscribe<AcceptQuestMessage>(OnAcceptQuest, this.GetType().Name);
			pubsub.Subscribe<ResponseQuestTrackingMessage>(OnQuestTracking, this.GetType().Name);
		}

		private void OnQuestTracking(ResponseQuestTrackingMessage data)
		{
			DI.Instance.Resolve<ICurrentContext>().QuestTracking = data.QuestTracking;
		}

		private void OnAcceptQuest(AcceptQuestMessage data)
		{
			// Async without await in unity still blocks UI, so we have to create our own Task to make this work
			Task.Run(() =>
			{
				client.Workflow.SendAcceptQuestMessage(data.QuestName);
			});
		}

		private void OnRequestQuestTracking(RequestQuestTracking data)
		{
			// Async without await in unity still blocks UI, so we have to create our own Task to make this work
			Task.Run(() =>
			{
				client.Workflow.SendRequestQuestTrackingMessage();
			});
		}

		public async Task<bool> ConnectAsync(string host, int port)
		{
			return await client.ConnectAsync(host, port);
		}
	}
}
