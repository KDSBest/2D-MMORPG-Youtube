using Common.Extensions;
using Common.Protocol.Character;
using Common.Protocol.Combat;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Client.Workflow
{
	public class CharacterWorkflow : BaseJwtWorkflow, IWorkflow
	{

		public override async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
		}

		public override async Task OnLatencyUpdateAsync(int latency)
		{
		}

		public override async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			var charMsg = new CharacterMessage();
			if(charMsg.Read(reader))
			{
				PubSub.Publish(charMsg);
				return;
			}

			var expMsg = new ExpMessage();
			if (expMsg.Read(reader))
			{
				PubSub.Publish(expMsg);
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
