using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Collections.Concurrent;
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

        public async Task UpdateAsync()
        {
            await UdpManager.PollEventsAsync();
        }

        public async Task OnNetworkReceiveAsync(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
            if (reader.AvailableBytes == 0)
                return;

            await Workflows[peer.ConnectId].OnReceiveAsync(reader, channel);
        }

        public async Task OnPeerDisconnectedAsync(UdpPeer peer, DisconnectInfo disconnectInfo)
        {
            IWorkflow workflow;
            if(Workflows.TryRemove(peer.ConnectId, out workflow))
			{
                await workflow.OnDisconnectedAsync(disconnectInfo);
            }
        }

        public async Task OnNetworkErrorAsync(UdpEndPoint endPoint, int socketErrorCode)
        {
            Console.WriteLine($"Error {endPoint.Host}:{endPoint.Port} - {socketErrorCode}");
        }

        public async Task OnNetworkLatencyUpdateAsync(UdpPeer peer, int latency)
        {
            await Workflows[peer.ConnectId].OnLatencyUpdateAsync(latency);
        }

        public async Task OnNetworkReceiveAckAsync(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
        }

        public async Task OnNetworkReceiveUnconnectedAsync(UdpEndPoint remoteEndPoint, UdpDataReader reader)
        {
        }

        public async Task OnPeerConnectedAsync(UdpPeer peer)
        {
			IWorkflow wf = new T
			{
				UdpManager = this.UdpManager,
                SwitchWorkflowAsync = SwitchWorkflowAsync
			};

			await SwitchWorkflowAsync(peer, wf);
        }

        public virtual void OnWorkflowSwitch(UdpPeer peer, IWorkflow newWorkflow)
		{

		}

        private async Task SwitchWorkflowAsync(UdpPeer peer, IWorkflow newWorkflow)
		{
            newWorkflow.UdpManager = this.UdpManager;
            newWorkflow.SwitchWorkflowAsync = SwitchWorkflowAsync;
            await newWorkflow.OnStartAsync(peer);
            Workflows.AddOrUpdate(peer.ConnectId, newWorkflow, (connectionId, oldValue) => newWorkflow);
            OnWorkflowSwitch(peer, newWorkflow);
        }
    }
}
