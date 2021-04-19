using Common.Client.Workflow;
using Common.Protocol.Login;
using Common.Workflow;
using ReliableUdp;
using System.Threading.Tasks;

namespace Common.Client
{

	public class LoginClient : BaseClient<CryptoWorkflow<LoginWorkflow>>
	{

		public bool IsConnectedAndLoginWorkflow
		{
			get
			{
				return IsConnected && Workflow != null;
			}
		}

		public LoginWorkflow Workflow { get; set; }

		public override void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow)
		{
			Workflow = newWorkflow as LoginWorkflow;
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
