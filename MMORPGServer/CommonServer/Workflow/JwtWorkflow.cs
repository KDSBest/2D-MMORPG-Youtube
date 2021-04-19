using Common.Protocol.Chat;
using Common.Protocol.Crypto;
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
using System.Text;
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
				}
			}
		}

		public void OnToken(string token)
		{
		}
	}
}
