using Common.Extensions;
using Common.IoC;
using Common.Protocol.Login;
using Common.PublishSubscribe;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Threading.Tasks;

namespace Common.Client.Workflow
{
	public class LoginWorkflow : IWorkflow
	{
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }

		public IPubSub PubSub { get; set; }

		public async Task OnStartAsync(UdpPeer peer)
		{
			PubSub = DI.Instance.Resolve<IPubSub>();
		}

		public async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
		}

		public async Task OnLatencyUpdateAsync(int latency)
		{
		}

		public async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			var response = new LoginRegisterResponseMessage();
			if(response.Read(reader))
			{
				PubSub.Publish(response);
			}
		}

		public async Task LoginAsync(string email, string password)
		{
			var loginMsg = new LoginMessage
			{
				EMail = email,
				Password = password
			};
			UdpManager.SendMsg(loginMsg, ChannelType.ReliableOrdered);
		}

		public async Task RegisterAsync(string email, string password)
		{
			var loginMsg = new RegisterMessage
			{
				EMail = email,
				Password = password
			};
			UdpManager.SendMsg(loginMsg, ChannelType.ReliableOrdered);
		}
	}
}
