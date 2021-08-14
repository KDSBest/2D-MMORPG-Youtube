using Assets.Scripts.PubSubEvents.LoginClient;
using Common.Client.Interfaces;
using Common.IoC;
using Common.Protocol.Inventory;
using Common.Protocol.PlayerEvent;
using Common.PublishSubscribe;
using System;
using System.Threading.Tasks;

namespace Assets.Scripts.ClientWrappers
{
	public class InventoryClientWrapper : IInventoryClientWrapper
	{
		public IInventoryClient client;
		private DateTime LastRequestTime = DateTime.MinValue;

		public bool IsInitialized { get { return client.IsConnected; } }

		private IPubSub pubsub;
		public InventoryClientWrapper()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
			client = DI.Instance.Resolve<IInventoryClient>();
			pubsub.Subscribe<RequestInventoryMessage>(OnRequestInventory, this.GetType().Name);
			pubsub.Subscribe<PlayerEventMessage>(OnPlayerEvent, this.GetType().Name);
		}

		private void OnPlayerEvent(PlayerEventMessage ev)
		{
			OnRequestInventory(new RequestInventoryMessage());
		}

		private void OnRequestInventory(RequestInventoryMessage data)
		{
			// we don't request Inventory too fast
			if ((DateTime.UtcNow - LastRequestTime).TotalMilliseconds < 100)
				return;

			LastRequestTime = DateTime.UtcNow;

			// Async without await in unity still blocks UI, so we have to create our own Task to make this work
			Task.Run(() =>
			{
				client.Workflow.SendRequestInventory();
			});
		}

		public async Task<bool> ConnectAsync(string host, int port)
		{
			return await client.ConnectAsync(host, port);
		}
	}
}
