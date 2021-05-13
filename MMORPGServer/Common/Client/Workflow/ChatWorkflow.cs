using Common.Extensions;
using Common.IoC;
using Common.Protocol.Chat;
using Common.PublishSubscribe;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Threading.Tasks;

namespace Common.Client.Workflow
{
	public class ChatWorkflow : IWorkflow
	{
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }

		public IPubSub PubSub { get; set; }

		public async Task OnStartAsync(UdpPeer peer)
		{
			PubSub = DI.Instance.Resolve<IPubSub>();
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
			if (chatMsg.Read(reader))
			{
				PubSub.Publish(chatMsg);
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
