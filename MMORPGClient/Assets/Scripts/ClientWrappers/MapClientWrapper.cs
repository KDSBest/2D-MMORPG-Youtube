using Assets.Scripts.PubSubEvents.MapClient;
using Common.Client.Interfaces;
using Common.IoC;
using Common.Protocol.Map;
using Common.PublishSubscribe;
using System;
using System.Threading.Tasks;

namespace Assets.Scripts.ClientWrappers
{
	public class MapClientWrapper : IMapClientWrapper
	{
		public IMapClient client;

		public bool IsInitialized { get { return client.IsConnected; } }

		private IPubSub pubsub;

		public MapClientWrapper()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
			client = DI.Instance.Resolve<IMapClient>();
			pubsub.Subscribe<PlayerState>(OnPlayerStateSend, this.GetType().Name);
			pubsub.Subscribe<TeleportMessage>(OnTeleport, this.GetType().Name);
		}

		private void OnTeleport(TeleportMessage msg)
		{
			if (IsInitialized)
			{
				Task.Run(() =>
				{
					client.Workflow.SendTeleportTo(msg.Name);
				});
			}
		}

		private void OnPlayerStateSend(PlayerState data)
		{
			if (IsInitialized)
			{
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
