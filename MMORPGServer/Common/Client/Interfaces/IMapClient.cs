using Common.Client.Workflow;
using Common.Protocol.Map;
using Common.Workflow;
using ReliableUdp;
using System;

namespace Common.Client.Interfaces
{
	public interface IMapClient : IBaseClient
	{
		MapWorkflow Workflow { get; set; }

		void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow);
	}
}