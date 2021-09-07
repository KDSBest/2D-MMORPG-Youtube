using Common.Client.Interfaces;
using Common.IoC;
using Common.Protocol.Combat;
using Common.PublishSubscribe;
using System;
using System.Threading.Tasks;

namespace Assets.Scripts.ClientWrappers
{
	public class CombatClientWrapper : ICombatClientWrapper
	{
		public ICombatClient client;
		private const string PUBSUBNAME = "CombatClientWrapper";

		public bool IsInitialized { get { return client.IsConnected; } }

		private IPubSub pubsub;
		public CombatClientWrapper()
		{
			DILoader.Initialize();
			pubsub = DI.Instance.Resolve<IPubSub>();
			client = DI.Instance.Resolve<ICombatClient>();

			pubsub.Subscribe<ReqSkillCastMessage>(OnReqSkillCastMessageSend, this.GetType().Name);
		}

		private void OnReqSkillCastMessageSend(ReqSkillCastMessage msg)
		{
			// Async without await in unity still blocks UI, so we have to create our own Task to make this work
			Task.Run(() =>
			{
				client.Workflow.SendReqSkillCastMessage(msg);
			});
		}

		public async Task<bool> ConnectAsync(string host, int port)
		{
			return await client.ConnectAsync(host, port);
		}
	}
}
