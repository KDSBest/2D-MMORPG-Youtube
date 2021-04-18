using Common.Client.Workflow;
using Common.Protocol.Login;
using Common.Udp;
using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Client
{

	public class LoginClient : BaseClient<BaseUdpListener<CryptoWorkflow<LoginWorkflow>>, CryptoWorkflow<LoginWorkflow>>
	{

		public bool IsConnectedAndLoginWorkflow
		{
			get
			{
				return IsConnected && Workflow != null;
			}
		}

		public LoginWorkflow Workflow
		{
			get
			{
				return this.UdpListener.Workflows[this.Peer.ConnectId] as LoginWorkflow;
			}
		}

		public async Task<LoginRegisterResponseMessage> LoginAsync(string email, string password)
		{
			var wf = Workflow;
			if (wf == null)
				return null;

			LoginRegisterResponseMessage response = null;
			await wf.LoginAsync(email, password, (resp) =>
			{
				response = resp;
			});

			WaitForResponse(() => response != null);

			return response;
		}

		public async Task<LoginRegisterResponseMessage> RegisterAsync(string email, string password)
		{
			var wf = Workflow;
			if (wf == null)
				return null;

			LoginRegisterResponseMessage response = null;
			await wf.RegisterAsync(email, password, (resp) =>
			{
				response = resp;
			});

			WaitForResponse(() => response != null);

			return response;
		}

	}
}
