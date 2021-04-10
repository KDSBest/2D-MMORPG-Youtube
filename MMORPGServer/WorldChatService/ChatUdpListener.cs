using Common.Protocol.Chat;
using CommonServer.Udp;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldChatService
{
    public class ChatUdpListener : BaseUdpListener, IUdpListener
    {
        public override void OnNetworkReceive(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
            var chatMessage = new ChatMessage();
            if(chatMessage.Read(reader))
            {
                this.SendMsg(chatMessage, channel);
            }
        }
    }
}
