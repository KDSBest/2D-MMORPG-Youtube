using Common.Crypto;
using Common.Extensions;
using Common.Protocol.Chat;
using Common.Protocol.Login;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Client.Workflow
{
	public class ChatWorkflow : IWorkflow
	{
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }

		public Action<ChatMessage> OnNewChatMessage { get; set; }

		public async Task OnStartAsync(UdpPeer peer)
		{
		}

		public async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
		}

		public async Task OnLatencyUpdateAsync(int latency)
		{
		}

		public async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			var chatMsg = new ChatMessage();
			if(chatMsg.Read(reader))
			{
				OnNewChatMessage?.Invoke(chatMsg);
			}
		}

		public void SendChatMessage(string message)
		{
			var msg = new ChatMessage
			{
				Message = message
			};
			UdpManager.SendMsg(msg, ChannelType.Reliable);
		}
	}
}
