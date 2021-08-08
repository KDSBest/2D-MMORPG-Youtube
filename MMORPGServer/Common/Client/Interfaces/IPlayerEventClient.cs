using Common.Client.Workflow;
using Common.Workflow;
using ReliableUdp;

namespace Common.Client.Interfaces
{
	public interface IPlayerEventClient : IBaseClient
	{
		PlayerEventWorkflow Workflow { get; set; }

		void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow);
	}
}