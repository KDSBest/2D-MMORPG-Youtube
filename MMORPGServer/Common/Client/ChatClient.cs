﻿using Common.Client.Workflow;
using Common.Protocol.Chat;
using Common.Workflow;
using ReliableUdp;
using System;

namespace Common.Client
{

	public class ChatClient : BaseClient<ChatWorkflow>
	{
		public override bool IsConnected
		{
			get
			{
				return base.IsConnected && Workflow != null;
			}
		}

		public ChatWorkflow Workflow { get; set; }

		public override void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow)
		{
			Workflow = newWorkflow as ChatWorkflow;
		}

	}
}