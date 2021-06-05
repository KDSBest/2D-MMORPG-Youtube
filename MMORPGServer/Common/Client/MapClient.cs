using Common.Client.Interfaces;
using Common.Client.Workflow;
using Common.Workflow;
using ReliableUdp;

namespace Common.Client
{

	public class MapClient : BaseClient<MapWorkflow>, IMapClient
	{
		public override bool IsConnected
		{
			get
			{
				return base.IsConnected && Workflow != null;
			}
		}

		public MapWorkflow Workflow { get; set; }

		public override void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow)
		{
			Workflow = newWorkflow as MapWorkflow;
		}
	}
}
