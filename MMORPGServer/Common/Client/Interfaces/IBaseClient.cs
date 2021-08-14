using Common.Workflow;
using ReliableUdp;
using System.Threading.Tasks;

namespace Common.Client.Interfaces
{
	public interface IBaseClient<TWorkflow> where TWorkflow : IWorkflow
	{
		TWorkflow Workflow { get; set; }

		bool IsConnected { get; }

		UdpPeer Peer { get; set; }

		Task<bool> ConnectAsync(string host = "localhost", int port = 3334);
		Task DisconnectAsync();
		Task PollEventsAsync();

		void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow);

	}
}