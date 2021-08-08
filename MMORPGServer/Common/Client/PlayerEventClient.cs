using Common.Client.Interfaces;
using Common.Client.Workflow;
using Common.Workflow;
using ReliableUdp;

namespace Common.Client
{

	public class PlayerEventClient : BaseClient<PlayerEventWorkflow>, IPlayerEventClient
	{
		public override bool IsConnected
		{
			get
			{
				return base.IsConnected && Workflow != null;
			}
		}

		public PlayerEventWorkflow Workflow { get; set; }

		public override void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow)
		{
			Workflow = newWorkflow as PlayerEventWorkflow;
		}

	}
}
