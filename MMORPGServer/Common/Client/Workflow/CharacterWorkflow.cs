using Common.Extensions;
using Common.IoC;
using Common.Protocol.Character;
using Common.PublishSubscribe;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Client.Workflow
{
	public class CharacterWorkflow : IWorkflow
	{
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }
		public IPubSub PubSub { get; set; }

		public async Task OnStartAsync(UdpPeer peer)
		{
			PubSub = DI.Instance.Resolve<IPubSub>();
		}

		public async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
		}

		public async Task OnLatencyUpdateAsync(int latency)
		{
		}

		public async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			var charMsg = new CharacterMessage();
			if(charMsg.Read(reader))
			{
				PubSub.Publish(charMsg);
				return;
			}
		}

		public void SendCharacterRequest(List<string> names)
		{
			var msg = new CharacterRequestMessage
			{
				Names = names
			};
			UdpManager.SendMsg(msg, ChannelType.Reliable);
		}

		public void SendCharacterCreation(CharacterInformation c)
		{
			var msg = new CharacterMessage
			{
				Character = c
			};
			UdpManager.SendMsg(msg, ChannelType.Reliable);
		}
	}
}
