using Common.Workflow;
using CommonServer;
using CommonServer.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using CommonServer.Configuration;
using System.Threading.Tasks;
using Common.Protocol.Character;
using CommonServer.CosmosDb;
using Common.Extensions;
using System.Collections.Generic;
using System.Security.Claims;
using CommonServer.Redis;
using Common.Protocol.Combat;
using StackExchange.Redis;
using Common.GameDesign;
using CommonServer.Redis.Model;

namespace CharacterService
{
	public class CharacterWorkflow : IJwtWorkflow
	{
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }
		private string playerEmail = string.Empty;
		private CharacterInformationRepository repo = new CharacterInformationRepository();
		private UdpPeer peer;
		private string loggedInChar = string.Empty;

		public async Task OnStartAsync(UdpPeer peer)
		{
			this.peer = peer;

			if (await repo.HasPlayerACharacterAsync(playerEmail))
			{
				await SendPlayerHisCharacterAsync(true);
			}
			else
			{
				SendPlayerCharacter(new CharacterInformation());
			}
		}

		private async Task SendPlayerHisCharacterAsync(bool sendToken)
		{
			CharacterInformation c = await repo.GetCharacterByOwnerAsync(playerEmail);
			string token = string.Empty;

			if (sendToken)
			{
				RedisQueue.Enqueue<LoginQueueItem>(RedisConfiguration.LoginQueue, new LoginQueueItem()
				{
					PlayerId = c.Name,
					LoginTime = DateTime.Now
				});

				if (string.IsNullOrEmpty(loggedInChar) || loggedInChar != c.Name)
				{
					RedisPubSub.Subscribe<ExpMessage>(RedisConfiguration.PlayerExpPrefix + c.Name, OnExpGain);
					RedisPubSub.Subscribe<DamageMessage>(RedisConfiguration.PlayerDamagePrefix + c.Name, OnDamageReceived);

					loggedInChar = c.Name;
				}

				token = JwtTokenHelper.GenerateToken(new List<Claim>
				{
					new Claim(SecurityConfiguration.EmailClaimType, playerEmail),
					new Claim(SecurityConfiguration.CharClaimType, c.Name)
				});
			}

			SendPlayerCharacter(c, token);
		}

		private void OnDamageReceived(RedisChannel channel, DamageMessage msg)
		{
			if (msg.Target.TargetType != SkillCastTargetType.SingleTarget || msg.Target.TargetName != this.loggedInChar)
				return;

			CharacterInformation c = repo.GetAsync(loggedInChar).Result;
			c.Stats.HP -= msg.DamageInfo.Damage;

			if (c.Stats.HP < 0)
				c.Stats.HP = 0;

			repo.SaveAsync(c, loggedInChar).Wait();

			SendCharacterUpdate(c.Stats);
		}

		private async Task SendPlayerCharacterAsync(string charName)
		{
			var c = await repo.GetAsync(charName);

			if (c != null)
			{
				SendPlayerCharacter(c);
			}
		}

		private void SendPlayerCharacter(CharacterInformation myChar, string token = "")
		{
			var charMessage = new CharacterMessage();
			charMessage.Character = myChar;
			charMessage.Token = token;
			UdpManager.SendMsg(this.peer.ConnectId, charMessage, ChannelType.Reliable);
		}

		public async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
			if (string.IsNullOrEmpty(loggedInChar))
				return;

			RedisPubSub.UnSubscribe(RedisConfiguration.PlayerExpPrefix + loggedInChar);
		}

		public async Task OnLatencyUpdateAsync(int latency)
		{
		}

		public async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			var charReq = new CharacterRequestMessage();
			if (charReq.Read(reader))
			{
				foreach (string name in charReq.Names)
				{
					await SendPlayerCharacterAsync(name);
				}
				return;
			}

			var charMessage = new CharacterMessage();
			if (charMessage.Read(reader))
			{
				charMessage.Character.Id = charMessage.Character.Name = charMessage.Character.Name.Trim();
				charMessage.Character.Owner = playerEmail;
				if (string.IsNullOrEmpty(charMessage.Character.Name))
				{
					SendPlayerCharacter(charMessage.Character);
					return;
				}

				if (await repo.HasPlayerACharacterAsync(playerEmail))
				{
					await SendPlayerHisCharacterAsync(false);
					return;
				}

				if (await repo.GetAsync(charMessage.Character.Name) != null)
				{
					charMessage.Character.Name = "NameAlreadyRegistered!";
					SendPlayerCharacter(charMessage.Character);
					return;
				}

				await repo.SaveAsync(charMessage.Character, charMessage.Character.Id);
				await SendPlayerHisCharacterAsync(true);
				return;
			}
		}

		public void OnToken(string token)
		{
			playerEmail = JwtTokenHelper.GetTokenClaim(token, SecurityConfiguration.EmailClaimType);
		}

		private void OnExpGain(RedisChannel channel, ExpMessage msg)
		{
			bool updateOtherServices = false;
			CharacterInformation c = repo.GetAsync(loggedInChar).Result;
			c.Experience += msg.ExpGain;

			while (c.Stats.Level < ExpCurve.MaxLevel && c.Experience >= ExpCurve.FullExp[c.Stats.Level])
			{
				//TODO update full Stats
				c.Stats.MaxHP += 100;
				c.Stats.HP += 100;
				c.Stats.Level++;
				SendPlayerCharacter(c);
				updateOtherServices = true;
			}

			UdpManager.SendMsg(this.peer.ConnectId, msg, ChannelType.ReliableOrdered);
			repo.SaveAsync(c, loggedInChar).Wait();

			if (updateOtherServices)
			{
				SendCharacterUpdate(c.Stats);
			}
		}

		private void SendCharacterUpdate(EntityStats stats)
		{
			RedisPubSub.Publish<UpdateCharacterMessage>(RedisConfiguration.CharUpdatePrefix + loggedInChar, new UpdateCharacterMessage()
			{
				Name = loggedInChar,
				Stats = stats
			});
		}
	}
}
