using Common.Client.Workflow;
using Common.Protocol.Character;
using Common.Protocol.Chat;
using Common.Protocol.Map;
using Common.Workflow;
using ReliableUdp;
using System;
using System.Collections.Generic;

namespace Common.Client
{

	public class MapClient : BaseClient<MapWorkflow>
	{
		public override bool IsConnected
		{
			get
			{
				return base.IsConnected && Workflow != null;
			}
		}

		public MapWorkflow Workflow { get; set; }

		public Action<PlayerStateMessage> OnNewPlayerStateMessage{ get; set; }

		public override void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow)
		{
			Workflow = newWorkflow as MapWorkflow;
		}
	}
}
