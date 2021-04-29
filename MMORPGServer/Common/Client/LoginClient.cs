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
		private IPubSub pubsub;

		public bool IsConnectedAndLoginWorkflow
		{
			get
			{
				return IsConnected && Workflow != null;
			}
		}

		public LoginClient(IPubSub pubsub)
		{
			this.pubsub = pubsub;
		}

		public LoginWorkflow Workflow { get; set; }

		public override void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow)
		{
			Workflow = newWorkflow as LoginWorkflow;
			if(Workflow != null)
			{
				Workflow.PubSub = pubsub;
			}
		}

		public async Task LoginAsync(string email, string password)
		{
			var wf = Workflow;
			if (wf == null)
				return;

			await wf.LoginAsync(email, password);
		}

		public async Task RegisterAsync(string email, string password)
		{
			var wf = Workflow;
			if (wf == null)
				return;

			await wf.RegisterAsync(email, password);
		}

	}
}
