using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Protocol.Chat;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;

namespace ChatClient
{
    public class ChatClientListener : IUdpEventListener
    {
        public UdpManager UdpManager { get; set; }

        public void OnNetworkError(UdpEndPoint endPoint, int socketErrorCode)
        {
        }

        public void OnNetworkLatencyUpdate(UdpPeer peer, int latency)
        {
        }

        public void OnNetworkReceive(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
            var chatMessage = new ChatMessage();
            if(chatMessage.Read(reader))
            {
                Console.WriteLine(chatMessage.Message);
            }
        }

        public void OnNetworkReceiveAck(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
        }

        public void OnNetworkReceiveUnconnected(UdpEndPoint remoteEndPoint, UdpDataReader reader)
        {
        }

        public void OnPeerConnected(UdpPeer peer)
        {
        }

        public void OnPeerDisconnected(UdpPeer peer, DisconnectInfo disconnectInfo)
        {
        }
    }
}
