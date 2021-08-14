using Common.Client.Workflow;
using Common.Workflow;
using ReliableUdp;

namespace Common.Client
{
	public class CryptoBaseClient<TWorkflow> : BaseClient<CryptoWorkflow<TWorkflow>> where TWorkflow : class, ICryptoWorkflow, new()
	{
		public TWorkflow WorkflowAfterCrypto { get; set; }

		public override bool IsConnected
		{
			get
			{
				return base.IsConnected && WorkflowAfterCrypto != null;
			}
		}

		public override void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow)
		{
			base.OnWorkflowSwitch(peer, newWorkflow);

			WorkflowAfterCrypto = newWorkflow as TWorkflow;
		}
	}
}
