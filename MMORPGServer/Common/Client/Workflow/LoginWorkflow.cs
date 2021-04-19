using Common.Crypto;
using Common.Extensions;
using Common.Protocol.Login;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Client.Workflow
{
	public class LoginWorkflow : ICryptoWorkflow
	{
		public CryptoProvider Crypto { get; set; }
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }

		private Action<LoginRegisterResponseMessage> callback;

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
				callback?.Invoke(response);
			}
		}

		public async Task LoginAsync(string email, string password, Action<LoginRegisterResponseMessage> callback)
		{
			var loginMsg = new LoginMessage
			{
				EMailEnc = Crypto.Encrypt(email),
				PasswordEnc = Crypto.Encrypt(password)
			};
			UdpManager.SendMsg(loginMsg, ChannelType.ReliableOrdered);
			this.callback = callback;
		}

		public async Task RegisterAsync(string email, string password, Action<LoginRegisterResponseMessage> callback)
		{
			var loginMsg = new RegisterMessage
			{
				EMailEnc = Crypto.Encrypt(email),
				PasswordEnc = Crypto.Encrypt(password)
			};
			UdpManager.SendMsg(loginMsg, ChannelType.ReliableOrdered);
			this.callback = callback;
		}
	}
}
