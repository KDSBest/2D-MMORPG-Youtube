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
		public Action<UdpPeer, IWorkflow> SwitchWorkflow { get; set; }

		private UdpPeer peer;

		public void OnStart(UdpPeer peer)
		{
			this.peer = peer;
		}

		public void OnDisconnected(DisconnectInfo disconnectInfo)
		{
		}

		public void OnLatencyUpdate(int latency)
		{
		}

		public void OnReceive(UdpDataReader reader, ChannelType channel)
		{
			var tokenMessage = new JwtMessage();
			if (tokenMessage.Read(reader))
			{
				IJwtWorkflow wf = new T();
				if (JwtTokenHelper.ValidateToken(tokenMessage.Token))
				{
					this.OnToken(tokenMessage.Token);
					wf.OnToken(tokenMessage.Token);
					SwitchWorkflow(this.peer, wf);
				}
			}
		}

		public void OnToken(string token)
		{
		}
	}
}
