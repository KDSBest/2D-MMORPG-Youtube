using Common.Protocol.Chat;
using Common.Workflow;
using CommonServer;
using CommonServer.Redis;
using CommonServer.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WorldChatService
{
	public class ChatWorkflow : IJwtWorkflow
	{
		public UdpManager UdpManager { get; set; }
		public Action<UdpPeer, IWorkflow> SwitchWorkflow { get; set; }
		private string email = string.Empty;

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
			var chatMessage = new ChatMessage();
			if (chatMessage.Read(reader))
			{
				chatMessage.Sender = email;
				RedisPubSub.Publish<ChatMessage>(RedisConfiguration.WorldChatChannelPrefix, chatMessage);
			}
		}

		public void OnToken(string token)
		{
			email = JwtTokenHelper.GetTokenClaim(token, SecurityConfiguration.EmailClaimType);
		}
	}
}
