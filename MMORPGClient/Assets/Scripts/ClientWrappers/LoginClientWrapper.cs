using Assets.Scripts.PubSubEvents.LoginClient;
using Common.Client.Interfaces;
using Common.IoC;
using Common.PublishSubscribe;
using System.Threading.Tasks;

namespace Assets.Scripts.ClientWrappers
{
	public class LoginClientWrapper : ILoginClientWrapper
	{
		public ILoginClient client;
		private const string PUBSUBNAME = "LoginClientWrapper";

		public bool IsInitialized { get { return client.IsConnected; } }

		private IPubSub pubsub;
		public LoginClientWrapper()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
			client = DI.Instance.Resolve<ILoginClient>();
			pubsub.Subscribe<TryLogin>(OnTryLogin, PUBSUBNAME);
			pubsub.Subscribe<TryRegister>(OnTryRegister, PUBSUBNAME);
		}

		private void OnTryRegister(TryRegister data)
		{
			// Async without await in unity still blocks UI, so we have to create our own Task to make this work
			Task.Run(() =>
			{
				client.WorkflowAfterCrypto.RegisterAsync(data.Email, data.Password);
			});
		}

		private void OnTryLogin(TryLogin data)
		{
			// Async without await in unity still blocks UI, so we have to create our own Task to make this work
			Task.Run(() =>
			{
				client.WorkflowAfterCrypto.LoginAsync(data.Email, data.Password);
			});
		}

		public async Task<bool> ConnectAsync(string host, int port)
		{
			return await client.ConnectAsync(host, port);
		}
	}
}
