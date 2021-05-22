using Common.Client.Workflow;
using Common.Workflow;
using ReliableUdp;

namespace Common.Client.Interfaces
{
	public interface ILoginClient : IBaseClient
	{
		bool IsConnected { get; }
		LoginWorkflow Workflow { get; set; }

		void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow);
	}
}