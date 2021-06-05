using Assets.Scripts.PubSubEvents.ChatClient;
using Common.Client.Interfaces;
using Common.IoC;
using Common.PublishSubscribe;
using System.Threading.Tasks;

namespace Assets.Scripts.ClientWrappers
{
	public class ChatClientWrapper : IChatClientWrapper
	{
		public IChatClient client;
		private const string PUBSUBNAME = "ChatClientWrapper";

		public bool IsInitialized { get { return client.IsConnected; } }

		private IPubSub pubsub;
		public ChatClientWrapper()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
			client = DI.Instance.Resolve<IChatClient>();
			pubsub.Subscribe<SendChatMessage>(OnChatMessageSend, PUBSUBNAME);
		}

		private void OnChatMessageSend(SendChatMessage data)
		{
			// Async without await in unity still blocks UI, so we have to create our own Task to make this work
			Task.Run(() =>
			{
				client.Workflow.SendChatMessage(data.Message);
			});
		}

		public async Task<bool> ConnectAsync(string host, int port)
		{
			return await client.ConnectAsync(host, port);
		}
	}
}
