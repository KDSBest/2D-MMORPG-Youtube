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
using Common.GameDesign;

namespace CombatService
{
	public class CombatWorkflow : IJwtWorkflow
	{
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }

		private string playerId = string.Empty;
		private UdpPeer peer;
		private Dictionary<SkillCastType, DateTime> usedSkill = new Dictionary<SkillCastType, DateTime>();

		public async Task OnStartAsync(UdpPeer peer)
		{
			this.peer = peer;
		}

		public async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
			RedisPubSub.UnSubscribe(RedisConfiguration.PlayerDamagePrefix + playerId);
			RedisPubSub.UnSubscribe(RedisConfiguration.PlayerExpPrefix + playerId);
		}

		public async Task OnLatencyUpdateAsync(int latency)
		{
		}

		private bool CheckCooldown(ReqSkillCastMessage reqMsg)
		{
			if (usedSkill.ContainsKey(reqMsg.Type))
			{
				var elapsedTime = (DateTime.UtcNow - usedSkill[reqMsg.Type]).TotalMilliseconds + GameDesignConfiguration.CooldownLatencyAllowance;
				if (elapsedTime < GameDesignConfiguration.Skills.SkillTable[reqMsg.Type].GetStats(1).Cooldown)
					return false;

				usedSkill[reqMsg.Type] = DateTime.UtcNow;
			}
			else
			{
				usedSkill.Add(reqMsg.Type, DateTime.UtcNow);
			}
			return true;
		}

		public async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			var reqMsg = new ReqSkillCastMessage();
			if (reqMsg.Read(reader))
			{
				if (!CheckCooldown(reqMsg))
					return;

				RedisPubSub.Publish<SkillCastMessage>(RedisConfiguration.MapChannelSkillCastPrefix + MapConfiguration.MapName, new SkillCastMessage()
				{
					Caster = playerId,
					Type = reqMsg.Type,
					Position = reqMsg.Position,
					ServerTime = DateTime.UtcNow.Ticks,
					Target = reqMsg.Target
				});
			}
		}

		public void OnToken(string token)
		{
			playerId = JwtTokenHelper.GetTokenClaim(token, SecurityConfiguration.EmailClaimType);
			RedisPubSub.Subscribe<DamageMessage>(RedisConfiguration.PlayerDamagePrefix + playerId, OnDamageDone);
			RedisPubSub.Subscribe<ExpMessage>(RedisConfiguration.PlayerExpPrefix + playerId, OnExpGain);
		}

		private void OnExpGain(RedisChannel channel, ExpMessage msg)
		{
			UdpManager.SendMsg(this.peer.ConnectId, msg, ChannelType.Unreliable);
		}

		private void OnDamageDone(RedisChannel channel, DamageMessage msg)
		{
			UdpManager.SendMsg(this.peer.ConnectId, msg, ChannelType.Unreliable);
		}
	}
}
