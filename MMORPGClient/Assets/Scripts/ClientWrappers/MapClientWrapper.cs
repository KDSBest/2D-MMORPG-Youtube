using Assets.Scripts.PubSubEvents.MapClient;
using Common.Client.Interfaces;
using Common.IoC;
using Common.PublishSubscribe;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.ClientWrappers
{
	public class MapClientWrapper : IMapClientWrapper
	{
		public IMapClient client;
		private const string PUBSUBNAME = "MapClientWrapper";

		public bool IsInitialized { get { return client.IsConnected; } }

		private IPubSub pubsub;

		public MapClientWrapper()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
			client = DI.Instance.Resolve<IMapClient>();
			pubsub.Subscribe<PlayerState>(OnPlayerStateSend, PUBSUBNAME);
		}

		private void OnPlayerStateSend(PlayerState data)
		{
			if (IsInitialized)
			{
				// Async without await in unity still blocks UI, so we have to create our own Task to make this work
				Task.Run(() =>
				{
					client.Workflow.SendStateAsync(data.Position, data.Animation, data.IsLookingRight);
				});
			}
		}

		public async Task<bool> ConnectAsync(string host, int port)
		{
			return await client.ConnectAsync(host, port);
		}
	}
}
