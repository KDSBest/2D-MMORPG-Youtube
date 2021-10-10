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

namespace CharacterService
{
	public class CharacterWorkflow : IJwtWorkflow
	{
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }
		private string playerId = string.Empty;
		private CharacterInformationRepository repo = new CharacterInformationRepository();
		private UdpPeer peer;

		public async Task OnStartAsync(UdpPeer peer)
		{
			this.peer = peer;

			if (await repo.ExistsAsync(playerId))
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
			CharacterInformation c = await repo.GetAsync(playerId);
			string token = string.Empty;

			if (sendToken)
			{
				token = JwtTokenHelper.GenerateToken(new List<Claim>
				{
					new Claim(SecurityConfiguration.EmailClaimType, playerId),
					new Claim(SecurityConfiguration.CharClaimType, c.Name)
				});
			}

			SendPlayerCharacter(c, token);
		}

		private async Task SendPlayerCharacterAsync(string charName)
		{
			var c = await repo.GetByNameAsync(charName);

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
			RedisPubSub.UnSubscribe(RedisConfiguration.PlayerExpPrefix + playerId);
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
				charMessage.Character.Id = playerId;
				charMessage.Character.Name = charMessage.Character.Name.Trim();
				if (string.IsNullOrEmpty(charMessage.Character.Name))
				{
					SendPlayerCharacter(charMessage.Character);
					return;
				}

				if (await repo.ExistsAsync(playerId))
				{
					await SendPlayerHisCharacterAsync(false);
					return;
				}

				if (await repo.GetByNameAsync(charMessage.Character.Name) != null)
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
			playerId = JwtTokenHelper.GetTokenClaim(token, SecurityConfiguration.EmailClaimType);
			RedisPubSub.Subscribe<ExpMessage>(RedisConfiguration.PlayerExpPrefix + playerId, OnExpGain);
		}

		private void OnExpGain(RedisChannel channel, ExpMessage msg)
		{
			bool updateOtherServices = false;
			CharacterInformation c = repo.GetAsync(playerId).Result;
			c.Experience += msg.ExpGain;

			while (c.Level < ExpCurve.MaxLevel && c.Experience >= ExpCurve.FullExp[c.Level])
			{
				c.Level++;
				SendPlayerCharacter(c);
				updateOtherServices = true;
			}

			repo.SaveAsync(c, playerId).Wait();

			UdpManager.SendMsg(this.peer.ConnectId, msg, ChannelType.Reliable);

			if (updateOtherServices)
			{
				SendCharacterUpdate();
			}
		}

		private void SendCharacterUpdate()
		{
			RedisPubSub.Publish<UpdateCharacterMessage>(RedisConfiguration.CharUpdatePrefix + playerId, new UpdateCharacterMessage());
		}
	}
}
