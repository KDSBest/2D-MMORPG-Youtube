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
		public Action<UdpPeer, IWorkflow> SwitchWorkflow { get; set; }

		private Action<LoginRegisterResponseMessage> callback;

		public void OnStart(UdpPeer peer)
		{
		}

		public void OnDisconnected(DisconnectInfo disconnectInfo)
		{
		}

		public void OnLatencyUpdate(int latency)
		{
		}

		public void OnReceive(UdpDataReader reader, ChannelType channel)
		{
			var response = new LoginRegisterResponseMessage();
			if(response.Read(reader))
			{
				if (callback != null)
					callback(response);
			}
		}

		public async Task LoginAsync(string email, string password, Action<LoginRegisterResponseMessage> callback)
		{
			var loginMsg = new LoginMessage();
			loginMsg.EMailEnc = Crypto.Encrypt(email);
			loginMsg.PasswordEnc = Crypto.Encrypt(password);
			UdpManager.SendMsg(loginMsg, ChannelType.ReliableOrdered);
			this.callback = callback;
		}

		public async Task RegisterAsync(string email, string password, Action<LoginRegisterResponseMessage> callback)
		{
			var loginMsg = new RegisterMessage();
			loginMsg.EMailEnc = Crypto.Encrypt(email);
			loginMsg.PasswordEnc = Crypto.Encrypt(password);
			UdpManager.SendMsg(loginMsg, ChannelType.ReliableOrdered);
			this.callback = callback;
		}
	}
}
