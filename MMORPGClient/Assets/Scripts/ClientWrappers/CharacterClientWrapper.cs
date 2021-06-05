using Assets.Scripts.PubSubEvents.CharacterClient;
using Common.Client.Interfaces;
using Common.IoC;
using Common.Protocol.Character;
using Common.PublishSubscribe;
using System.Threading.Tasks;

namespace Assets.Scripts.ClientWrappers
{
	public class CharacterClientWrapper : ICharacterClientWrapper
	{
		public ICharacterClient client;
		private const string PUBSUBNAME = "CharacterClientWrapper";

		public bool IsInitialized { get { return client.IsConnected; } }

		private IPubSub pubsub;
		public CharacterClientWrapper()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
			pubsub.Subscribe<CharacterInformation>(OnNewCharacterInformation, PUBSUBNAME);
			pubsub.Subscribe<ReqCharacterStyle>(OnNewReqCharacterStyle, PUBSUBNAME);
			client = DI.Instance.Resolve<ICharacterClient>();
		}

		private void OnNewReqCharacterStyle(ReqCharacterStyle data)
		{
			if (data.Names.Count == 0)
				return;

			client.Workflow.SendCharacterRequest(data.Names);
		}

		private void OnNewCharacterInformation(CharacterInformation charInfo)
		{
			client.Workflow.SendCharacterCreation(charInfo);
		}

		public async Task<bool> ConnectAsync(string host, int port)
		{
			return await client.ConnectAsync(host, port);
		}
	}
}
