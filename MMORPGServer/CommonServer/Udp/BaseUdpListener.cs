using Common.Protocol;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CommonServer.Udp
{
    public abstract class BaseUdpListener : IUdpListener
    {
        public UdpManager UdpManager { get; set; }

        public virtual void Update()
        {
            UdpManager.PollEvents();
        }

        public virtual void OnMapIsEmpty(string mapName)
        {
        }
        public virtual void OnMapFirstConnection(string mapName)
        {
        }

        public virtual void OnNetworkReceive(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
        }

        public void OnPeerDisconnected(UdpPeer peer, DisconnectInfo disconnectInfo)
        {
        }

        protected void SendMsg(long id, IUdpPackage msg, ChannelType channelType)
        {
            UdpDataWriter writer = new UdpDataWriter();
            msg.Write(writer);

            UdpManager.SendTo(id, writer, channelType);
        }

        protected void SendMsg(IUdpPackage msg, ChannelType channelType)
        {
            if (UdpManager == null)
                return;

            UdpDataWriter writer = new UdpDataWriter();
            msg.Write(writer);

            UdpManager.SendToAll(writer, channelType);
        }

        public void OnNetworkError(UdpEndPoint endPoint, int socketErrorCode)
        {
        }

        public void OnNetworkLatencyUpdate(UdpPeer peer, int latency)
        {
        }

        public void OnNetworkReceiveAck(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
        }

        public void OnNetworkReceiveUnconnected(UdpEndPoint remoteEndPoint, UdpDataReader reader)
        {
        }

        public virtual void OnPeerConnected(UdpPeer peer)
        {
        }
    }
}
