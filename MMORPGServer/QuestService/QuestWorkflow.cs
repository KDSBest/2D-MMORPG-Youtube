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
using Common;
using System.Linq;
using Common.Protocol.Combat;

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
		private InventoryRepository invRepo = new InventoryRepository();
		private CharacterInformationRepository charRepo = new CharacterInformationRepository();
		private InventoryEventRepository invEventRepo = new InventoryEventRepository();

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
				await AcceptQuest(acceptQuestMsg);
				return;
			}

			FinishQuestMessage finishQuestMsg = new FinishQuestMessage();
			if (finishQuestMsg.Read(reader))
			{
				await FinishQuest(finishQuestMsg);
				return;
			}

			AbbandonQuestMessage abbondonQuestMsg = new AbbandonQuestMessage();
			if (abbondonQuestMsg.Read(reader))
			{
				await AbbandonQuest(abbondonQuestMsg);
				return;
			}

			RequestQuestTracking reqMsg = new RequestQuestTracking();
			if (reqMsg.Read(reader))
			{
				SendQuestTracking();
				return;
			}
		}

		private void SendQuestTracking()
		{
			ResponseQuestTrackingMessage resp = new ResponseQuestTrackingMessage()
			{
				QuestTracking = questTracking
			};

			UdpManager.SendMsg(this.peer.ConnectId, resp, ChannelType.ReliableOrdered);
		}

		private async Task FinishQuest(FinishQuestMessage finishQuestMsg)
		{
			if (!QuestLoader.Quests.ContainsKey(finishQuestMsg.QuestName))
			{
				return;
			}

			if (!questTracking.AcceptedQuests.Contains(finishQuestMsg.QuestName))
			{
				return;
			}

			DateTime eventTimestamp = DateTime.UtcNow;

			var lease = await invEventRepo.LeaseManagement.TryAcquireAsync(this.playerId, TimeSpan.FromSeconds(60), eventTimestamp);

			if (!lease.Acquired)
			{
				SendQuestTracking();
				return;
			}

			var quest = QuestLoader.Quests[finishQuestMsg.QuestName];
			this.questTracking.Inventory = await invRepo.GetClientInventoryAsync(this.playerId);

			if (!quest.Task.IsFinished(finishQuestMsg.QuestName, this.questTracking))
				return;

			await GiveReward(finishQuestMsg, quest);

			questTracking.AcceptedQuests.Remove(finishQuestMsg.QuestName);
			questTracking.FinishedQuests.Add(finishQuestMsg.QuestName);
			await repo.SaveAsync(questTracking, questTracking.Id);
			await invEventRepo.LeaseManagement.TryFreeAsync(lease);
			SendQuestTracking();
		}

		private async Task GiveReward(FinishQuestMessage finishQuestMsg, Quest quest)
		{
			var invQuestTasks = quest.Task.GetInventoryQuestTasks(finishQuestMsg.QuestName, this.questTracking);

			var expRewards = quest.Rewards.Where(x => x.ItemId == "Exp").ToList();
			foreach (var expReward in expRewards)
			{
				RedisPubSub.Publish<ExpMessage>(RedisConfiguration.PlayerExpPrefix + playerId, new ExpMessage()
				{
					ExpGain = expReward.Amount
				});
			}

			await invEventRepo.ApplyInventoryQuestRewardAsync(playerId, quest.Rewards.Where(x => x.ItemId != "Exp").ToList());

			// HINT: If we have multiple quest tasks on the same itemId this will not work so easily
			foreach (var invQuestTask in invQuestTasks)
			{
				await invEventRepo.ApplyInventoryQuestTaskAsync(playerId, invQuestTask);
			}
		}

		private async Task AcceptQuest(AcceptQuestMessage acceptQuestMsg)
		{
			if (!QuestLoader.Quests.ContainsKey(acceptQuestMsg.QuestName))
			{
				return;
			}

			var c = await charRepo.GetAsync(playerId);
			var quest = QuestLoader.Quests[acceptQuestMsg.QuestName];
			if (!quest.IsAvailable(questTracking, c.Level))
			{
				return;
			}

			questTracking.AcceptedQuests.Add(acceptQuestMsg.QuestName);

			await repo.SaveAsync(questTracking, questTracking.Id);
			
			SendQuestTracking();
		}

		private async Task AbbandonQuest(AbbandonQuestMessage msg)
		{
			if (!questTracking.AcceptedQuests.Contains(msg.QuestName))
			{
				SendQuestTracking();
				return;
			}

			questTracking.AcceptedQuests.Remove(msg.QuestName);

			await repo.SaveAsync(questTracking, questTracking.Id);

			SendQuestTracking();
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
