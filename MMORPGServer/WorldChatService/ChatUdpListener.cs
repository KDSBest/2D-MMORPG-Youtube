using Common.Extensions;
using Common.Protocol.Chat;
using Common.Udp;
using CommonServer;
using CommonServer.Redis;
using CommonServer.Workflow;
using ReliableUdp.Enums;
using StackExchange.Redis;
using CommonServer.Configuration;

namespace WorldChatService
{
	public class ChatUdpListener : BaseUdpListener<JwtWorkflow<ChatWorkflow>>, IUdpListener
    {
        public ChatUdpListener()
		{
            RedisPubSub.Subscribe<ChatMessage>(RedisConfiguration.WorldChatChannelPrefix, OnChatMessageReceived);
        }

        public void OnChatMessageReceived(RedisChannel channel, ChatMessage chatMessage)
		{
            this.UdpManager.SendMsg(chatMessage, ChannelType.Reliable);
		}
    }
}
