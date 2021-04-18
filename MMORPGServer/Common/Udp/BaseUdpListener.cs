using Common.Protocol;
using Common.Protocol.Crypto;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Udp
{
    public class BaseUdpListener<T> : IUdpListener where T : IWorkflow, new()
    {
        public ConcurrentDictionary<long, IWorkflow> Workflows = new ConcurrentDictionary<long, IWorkflow>();

		public UdpManager UdpManager { get; set; }

        public BaseUdpListener()
		{
		}

        public void Update()
        {
            UdpManager.PollEvents();
        }

        public void OnNetworkReceive(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
            Workflows[peer.ConnectId].OnReceive(reader, channel);
        }

        public void OnPeerDisconnected(UdpPeer peer, DisconnectInfo disconnectInfo)
        {
            Workflows[peer.ConnectId].OnDisconnected(disconnectInfo);
        }

        public void OnNetworkError(UdpEndPoint endPoint, int socketErrorCode)
        {
            Console.WriteLine($"Error {endPoint.Host}:{endPoint.Port} - {socketErrorCode}");
        }

        public void OnNetworkLatencyUpdate(UdpPeer peer, int latency)
        {
            Workflows[peer.ConnectId].OnLatencyUpdate(latency);
        }

        public void OnNetworkReceiveAck(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
        }

        public void OnNetworkReceiveUnconnected(UdpEndPoint remoteEndPoint, UdpDataReader reader)
        {
        }

        public void OnPeerConnected(UdpPeer peer)
        {
            IWorkflow wf = new T();

            wf.UdpManager = this.UdpManager;
            wf.SwitchWorkflow = SwitchWorkflow;

            SwitchWorkflow(peer, wf);
        }

        private void SwitchWorkflow(UdpPeer peer, IWorkflow newWorkflow)
		{
            newWorkflow.UdpManager = this.UdpManager;
            newWorkflow.SwitchWorkflow = SwitchWorkflow;
            newWorkflow.OnStart(peer);
            Workflows.AddOrUpdate(peer.ConnectId, newWorkflow, (connectionId, oldValue) => newWorkflow);
        }
    }
}
