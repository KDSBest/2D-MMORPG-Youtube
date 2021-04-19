using Common.Client.Workflow;
using Common.Protocol.Chat;
using Common.Protocol.Login;
using Common.Udp;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Client
{

	public class ChatClient : BaseClient<ChatWorkflow>
	{
		public ChatWorkflow Workflow { get; set; }

		public Action<ChatMessage> OnNewChatMessage { get; set; }

		public override void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow)
		{
			Workflow = newWorkflow as ChatWorkflow;
			if(Workflow != null)
			{
				Workflow.OnNewChatMessage = OnNewChatMessage;
			}
		}

		public void SendChatMessage(string message)
		{
			var wf = Workflow;
			if (wf == null)
				return;

			wf.SendChatMessage(message);
		}
	}
}
