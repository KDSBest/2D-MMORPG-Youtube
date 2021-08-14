using Assets.Scripts.PubSubEvents.LoginClient;
using Common.Client.Interfaces;
using Common.IoC;
using Common.Protocol.Inventory;
using Common.PublishSubscribe;
using System.Threading.Tasks;

namespace Assets.Scripts.ClientWrappers
{
	public class InventoryClientWrapper : IInventoryClientWrapper
	{
		public IInventoryClient client;
		private const string PUBSUBNAME = "InventoryClientWrapper";

		public bool IsInitialized { get { return client.IsConnected; } }

		private IPubSub pubsub;
		public InventoryClientWrapper()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
			client = DI.Instance.Resolve<IInventoryClient>();
			pubsub.Subscribe<RequestInventoryMessage>(OnRequestInventory, PUBSUBNAME);
		}

		private void OnRequestInventory(RequestInventoryMessage data)
		{
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
