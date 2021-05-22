using Common.Client.Interfaces;
using Common.Client.Workflow;
using Common.Protocol.Character;
using Common.Protocol.Chat;
using Common.Workflow;
using ReliableUdp;
using System;
using System.Collections.Generic;

namespace Common.Client
{

	public class CharacterClient : BaseClient<CharacterWorkflow>, ICharacterClient
	{
		public override bool IsConnected
		{
			get
			{
				return base.IsConnected && Workflow != null;
			}
		}

		public CharacterWorkflow Workflow { get; set; }

		public override void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow)
		{
			Workflow = newWorkflow as CharacterWorkflow;
		}
	}
}
