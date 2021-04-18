using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;

namespace Common.Workflow
{
	public interface IWorkflow
	{
		UdpManager UdpManager { get; set; }
		Action<UdpPeer, IWorkflow> SwitchWorkflow { get; set; }

		void OnStart(UdpPeer peer);
		void OnLatencyUpdate(int latency);
		void OnDisconnected(DisconnectInfo disconnectInfo);
		void OnReceive(UdpDataReader reader, ChannelType channel);
	}
}
