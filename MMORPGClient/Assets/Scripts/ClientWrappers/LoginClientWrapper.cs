using Assets.Scripts.PubSubEvents.LoginClient;
using Common.Client;
using Common.IoC;
using System.Threading.Tasks;

namespace Assets.Scripts.ClientWrappers
{
	public class LoginClientWrapper
	{
		public LoginClient loginClient = new LoginClient();
		private const string PUBSUBNAME = "LoginClientWrapper";

		public bool IsInitialized { get { return loginClient.IsConnectedAndLoginWorkflow; } }

		private IPubSub pubsub;
		public LoginClientWrapper()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
			pubsub.Subscribe<TryLogin>(OnTryLogin, PUBSUBNAME);
			pubsub.Subscribe<TryRegister>(OnTryRegister, PUBSUBNAME);
		}

		private void OnTryRegister(TryRegister data)
		{
			// Async without await in unity still blocks UI, so we have to create our own Task to make this work
			Task.Run(() =>
			{
				loginClient.RegisterAsync(data.Email, data.Password).ContinueWith((resp) =>
				{
					UnityDispatcher.RunOnMainThread(() =>
					{
						pubsub.Publish(resp.Result);
					});
				});
			});
		}

		private void OnTryLogin(TryLogin data)
		{
			// Async without await in unity still blocks UI, so we have to create our own Task to make this work
			Task.Run(() =>
			{
				loginClient.LoginAsync(data.Email, data.Password).ContinueWith((resp) =>
				{
					UnityDispatcher.RunOnMainThread(() =>
					{
						pubsub.Publish(resp.Result);
					});
				});
			});
		}

		public async Task<bool> ConnectAsync(string host, int port)
		{
			return await loginClient.ConnectAsync(host, port);
		}
	}
}
