using Common.Client.Workflow;
using Common.Protocol.Login;
using Common.PublishSubscribe;
using Common.Workflow;
using ReliableUdp;
using System.Threading.Tasks;

namespace Common.Client
{

	public class LoginClient : BaseClient<CryptoWorkflow<LoginWorkflow>>
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
