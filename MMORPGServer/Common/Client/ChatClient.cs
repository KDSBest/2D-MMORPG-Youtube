using Common.Client.Workflow;
using Common.Protocol.Login;
using Common.Udp;
using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Client
{

	public class ChatClient : BaseClient<BaseUdpListener<ChatWorkflow>, ChatWorkflow>
	{

		public ChatWorkflow Workflow
		{
			get
			{
				return this.UdpListener.Workflows[this.Peer.ConnectId] as ChatWorkflow;
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
