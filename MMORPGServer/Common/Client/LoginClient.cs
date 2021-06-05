using Common.Client.Interfaces;
using Common.Client.Workflow;
using Common.Workflow;
using ReliableUdp;

namespace Common.Client
{

	public class LoginClient : BaseClient<CryptoWorkflow<LoginWorkflow>>, ILoginClient
	{

		public override bool IsConnected
		{
			get
			{
				return base.IsConnected && Workflow != null;
			}
		}

		public LoginClient()
		{
		}

		public LoginWorkflow Workflow { get; set; }

		public override void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow)
		{
			Workflow = newWorkflow as LoginWorkflow;
		}

	}
}
