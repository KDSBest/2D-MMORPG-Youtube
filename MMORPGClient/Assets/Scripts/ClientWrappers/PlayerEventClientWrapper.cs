using Assets.Scripts.PubSubEvents.LoginClient;
using Common.Client.Interfaces;
using Common.IoC;
using Common.Protocol.Inventory;
using Common.PublishSubscribe;
using System.Threading.Tasks;

namespace Assets.Scripts.ClientWrappers
{
	public class PlayerEventClientWrapper : IPlayerEventClientWrapper
	{
		public IPlayerEventClient client;
		private const string PUBSUBNAME = "PlayerEventClientWrapper";

		public bool IsInitialized { get { return client.IsConnected; } }

		private IPubSub pubsub;
		public PlayerEventClientWrapper()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
			client = DI.Instance.Resolve<IPlayerEventClient>();
		}

		public async Task<bool> ConnectAsync(string host, int port)
		{
			return await client.ConnectAsync(host, port);
		}
	}
}
