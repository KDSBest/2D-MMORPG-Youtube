using Common.Protocol.Chat;
using Common.Workflow;
using CommonServer;
using CommonServer.Redis;
using CommonServer.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using CommonServer.Configuration;
using System.Threading.Tasks;

namespace WorldChatService
{
	public class ChatWorkflow : IJwtWorkflow
	{
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }
		private string name = string.Empty;

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
			var chatMessage = new ChatMessage();
			if (chatMessage.Read(reader))
			{
				chatMessage.Sender = name;
				RedisPubSub.Publish<ChatMessage>(RedisConfiguration.WorldChatChannelPrefix, chatMessage);
			}
		}

		public void OnToken(string token)
		{
			name = JwtTokenHelper.GetTokenClaim(token, SecurityConfiguration.CharClaimType);
		}
	}
}
