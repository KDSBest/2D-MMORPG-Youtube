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
		public Action<UdpPeer, IWorkflow> SwitchWorkflow { get; set; }

		public Action<ChatMessage> OnNewChatMessage { get; set; }

		public void OnStart(UdpPeer peer)
		{
		}

		public void OnDisconnected(DisconnectInfo disconnectInfo)
		{
		}

		public void OnLatencyUpdate(int latency)
		{
		}

		public void OnReceive(UdpDataReader reader, ChannelType channel)
		{
			var chatMsg = new ChatMessage();
			if(chatMsg.Read(reader))
			{
				if(OnNewChatMessage != null)
				{
					OnNewChatMessage(chatMsg);
				}
			}
		}

		public void SendChatMessage(string message)
		{
			var msg = new ChatMessage();
			msg.Message = message;
			UdpManager.SendMsg(msg, ChannelType.Reliable);
		}
	}
}
