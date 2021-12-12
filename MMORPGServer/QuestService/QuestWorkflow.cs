using Common.Protocol.Chat;
using Common.Workflow;
using CommonServer;
using CommonServer.Redis;
using CommonServer.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using CommonServer.Configuration;
using System.Threading.Tasks;
using Common.QuestSystem;
using CommonServer.CosmosDb;
using Common.Protocol.Quest;
using Common.Extensions;

namespace QuestService
{
	public class QuestWorkflow : IJwtWorkflow
	{
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }
		private string playerId = string.Empty;
		private UdpPeer peer;

		private QuestTrackingRepository repo = new QuestTrackingRepository();
		private QuestTracking questTracking = new QuestTracking();

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
			AcceptQuestMessage acceptQuestMsg = new AcceptQuestMessage();
			if (acceptQuestMsg.Read(reader))
			{
				QuestResultMessage resp;
				if (questTracking.AcceptedQuests.Contains(acceptQuestMsg.QuestName))
				{
					resp = new QuestResultMessage()
					{
						Id = acceptQuestMsg.QuestName,
						Result = false
					};

					UdpManager.SendMsg(this.peer.ConnectId, resp, ChannelType.ReliableOrdered);
					return;
				}

				questTracking.AcceptedQuests.Add(acceptQuestMsg.QuestName);
				
				await repo.SaveAsync(questTracking, questTracking.Id);

				resp = new QuestResultMessage()
				{
					Id = acceptQuestMsg.QuestName,
					Result = true
				};

				UdpManager.SendMsg(this.peer.ConnectId, resp, ChannelType.ReliableOrdered);
				return;
			}

			RequestQuestTracking reqMsg = new RequestQuestTracking();
			if (reqMsg.Read(reader))
			{
				ResponseQuestTrackingMessage resp = new ResponseQuestTrackingMessage()
				{
					QuestTracking = questTracking
				};

				UdpManager.SendMsg(this.peer.ConnectId, resp, ChannelType.ReliableOrdered);
				return;
			}
		}

		public void OnToken(string token)
		{
			playerId = JwtTokenHelper.GetTokenClaim(token, SecurityConfiguration.CharClaimType);

			questTracking = repo.GetAsync(playerId).Result;

			if (questTracking == null)
			{
				questTracking = new QuestTracking()
				{
					Id = playerId
				};
			}
		}
	}
}
