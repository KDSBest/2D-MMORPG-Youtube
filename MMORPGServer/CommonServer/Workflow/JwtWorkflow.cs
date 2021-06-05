using Common.Extensions;
using Common.Protocol.Crypto;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Threading.Tasks;

namespace CommonServer.Workflow
{
	public class JwtWorkflow<T> : IJwtWorkflow where T : IJwtWorkflow, new()
	{
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }

		private UdpPeer peer;

		public async Task OnStartAsync(UdpPeer peer)
		{
			this.peer = peer;
		}

		public async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
		}

		public async Task OnLatencyUpdateAsync(int latency)
		{
		}

		public async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			var tokenMessage = new JwtMessage();
			if (tokenMessage.Read(reader))
			{
				IJwtWorkflow wf = new T();
				if (JwtTokenHelper.ValidateToken(tokenMessage.Token))
				{
					this.OnToken(tokenMessage.Token);
					wf.OnToken(tokenMessage.Token);
					await SwitchWorkflowAsync(this.peer, wf);
					this.UdpManager.SendMsg(peer.ConnectId, tokenMessage, ChannelType.Reliable);
				}
				else
				{
					this.UdpManager.SendMsg(peer.ConnectId, new ReqJwtMessage(), ChannelType.Reliable);
				}
			}
			else
			{
				this.UdpManager.SendMsg(peer.ConnectId, new ReqJwtMessage(), ChannelType.Reliable);
			}
		}

		public void OnToken(string token)
		{
		}
	}
}
