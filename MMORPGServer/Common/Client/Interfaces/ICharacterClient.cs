using Common.Client.Workflow;
using Common.Workflow;
using ReliableUdp;

namespace Common.Client.Interfaces
{
	public interface ICharacterClient : IBaseClient
	{
		CharacterWorkflow Workflow { get; set; }
		void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow);
	}
}