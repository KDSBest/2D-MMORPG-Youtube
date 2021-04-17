using Common.Protocol.Chat;
using CommonServer;
using CommonServer.Redis;
using CommonServer.Udp;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldChatService
{
    public class ChatUdpListener : BaseUdpListener, IUdpListener
    {
        private Guid instanceId = Guid.NewGuid();

        public ChatUdpListener()
		{
            RedisPubSub.Subscribe<ChatMessage>(RedisConfiguration.WorldChatChannelPrefix, OnChatMessageReceived);
        }

        public void OnChatMessageReceived(RedisChannel channel, ChatMessage chatMessage)
		{
            this.SendMsg(chatMessage, ChannelType.Reliable);
		}

        public override void OnNetworkReceive(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
            var chatMessage = new ChatMessage();
            if(chatMessage.Read(reader))
            {
                chatMessage.InstanceId = instanceId;
                RedisPubSub.Publish<ChatMessage>(RedisConfiguration.WorldChatChannelPrefix, chatMessage);
            }
        }
    }
}
