using Assets.Scripts.PubSubEvents.LoginClient;
using Common.Client;
using Common.IoC;
using Common.Protocol.Character;
using Common.Protocol.Login;
using Common.PublishSubscribe;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.ClientWrappers
{
	public class CharacterClientWrapper
	{
		public CharacterClient client = new CharacterClient();
		private const string PUBSUBNAME = "CharacterClientWrapper";

		public bool IsInitialized { get { return client.IsConnected; } }

		private IPubSub pubsub;
		public CharacterClientWrapper()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
			pubsub.Subscribe<CharacterInformation>(OnNewCharacterInformation, PUBSUBNAME);
		}

		private void OnNewCharacterInformation(CharacterInformation charInfo)
		{
			client.Workflow.SendCharacterCreation(charInfo);
		}

		public async Task<bool> ConnectAsync(string host, int port, string token)
		{
			return await client.ConnectAsync(host, port, token);
		}
	}
}
