using Common.Extensions;
using Common.Protocol.Quest;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System.Threading.Tasks;

namespace Common.Client.Workflow
{
	public class QuestTrackingWorkflow : BaseJwtWorkflow, IWorkflow
	{

		public override async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
		}

		public override async Task OnLatencyUpdateAsync(int latency)
		{
		}

		public override async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			var questResultMessage = new QuestResultMessage();
			if (questResultMessage.Read(reader))
			{
				PubSub.Publish(questResultMessage);
				return;
			}

			var responseQuestTracking = new ResponseQuestTrackingMessage();
			if (responseQuestTracking.Read(reader))
			{
				PubSub.Publish(responseQuestTracking);
				return;
			}

			await base.OnReceiveAsync(reader, channel);
		}


		public void SendFinishQuestMessage(string questName)
		{
			var msg = new FinishQuestMessage
			{
				QuestName = questName
			};
			UdpManager.SendMsg(msg, ChannelType.Reliable);
		}

		public void SendAcceptQuestMessage(string questName)
		{
			var msg = new AcceptQuestMessage
			{
				QuestName = questName
			};
			UdpManager.SendMsg(msg, ChannelType.Reliable);
		}

		public void SendAbbandonQuestMessage(string questName)
		{
			var msg = new AbbandonQuestMessage
			{
				QuestName = questName
			};
			UdpManager.SendMsg(msg, ChannelType.Reliable);
		}

		public void SendRequestQuestTrackingMessage()
		{
			var msg = new RequestQuestTracking
			{
			};
			UdpManager.SendMsg(msg, ChannelType.Reliable);
		}
	}
}
