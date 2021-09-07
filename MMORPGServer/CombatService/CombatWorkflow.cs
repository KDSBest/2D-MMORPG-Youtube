using Common.Workflow;
using CommonServer;
using Common.Extensions;
using CommonServer.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using CommonServer.Configuration;
using System.Threading.Tasks;
using CommonServer.CosmosDb;
using Common.Protocol.Inventory;
using System.Collections.Generic;
using Common.Protocol.Combat;
using CommonServer.Redis;
using Common.Protocol.Map;
using Common;
using StackExchange.Redis;

namespace CombatService
{
	public class CombatWorkflow : IJwtWorkflow
	{
		private InventoryRepository repo = new InventoryRepository();
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }

		private string playerId = string.Empty;
		private UdpPeer peer;
		private Dictionary<string, DateTime> usedSkill = new Dictionary<string, DateTime>();

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
			var reqMsg = new ReqSkillCastMessage();
			if (reqMsg.Read(reader))
			{
				RedisPubSub.Publish<SkillCastMessage>(RedisConfiguration.MapChannelSkillCastPrefix + MapConfiguration.MapName, new SkillCastMessage()
				{
					Caster = playerId,
					Type = reqMsg.Type,
					DurationInMs = 2000,
					Position = reqMsg.Position,
					ServerTime = DateTime.UtcNow.Ticks,
					Target = reqMsg.Target
				});
			}
		}

		public void OnToken(string token)
		{
			playerId = JwtTokenHelper.GetTokenClaim(token, SecurityConfiguration.EmailClaimType);
			RedisPubSub.Subscribe<DamageDoneMessage>(RedisConfiguration.PlayerDamagePrefix + playerId, OnDamageDone);
		}

		private void OnDamageDone(RedisChannel channel, DamageDoneMessage dmg)
		{
			UdpManager.SendMsg(this.peer.ConnectId, dmg, ChannelType.Unreliable);
		}
	}
}
