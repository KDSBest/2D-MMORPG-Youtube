using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Threading.Tasks;

namespace Common.Workflow
{
	public interface IWorkflow
	{
		UdpManager UdpManager { get; set; }
		Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }

		Task OnStartAsync(UdpPeer peer);
		Task OnLatencyUpdateAsync(int latency);
		Task OnDisconnectedAsync(DisconnectInfo disconnectInfo);
		Task OnReceiveAsync(UdpDataReader reader, ChannelType channel);
	}
}
