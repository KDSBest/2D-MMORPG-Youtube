using Common.Crypto;
using Common.Extensions;
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
	public class LoginWorkflow : ICryptoWorkflow
	{
		public CryptoProvider Crypto { get; set; }
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }

		public IPubSub PubSub { get; set; }

		public async Task OnStartAsync(UdpPeer peer)
		{
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
				EMailEnc = Crypto.Encrypt(email),
				PasswordEnc = Crypto.Encrypt(password)
			};
			UdpManager.SendMsg(loginMsg, ChannelType.ReliableOrdered);
		}

		public async Task RegisterAsync(string email, string password)
		{
			var loginMsg = new RegisterMessage
			{
				EMailEnc = Crypto.Encrypt(email),
				PasswordEnc = Crypto.Encrypt(password)
			};
			UdpManager.SendMsg(loginMsg, ChannelType.ReliableOrdered);
		}
	}
}
