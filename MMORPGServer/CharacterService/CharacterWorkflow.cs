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
using Common.Protocol.Character;
using CommonServer.CosmosDb;
using Common.Extensions;
using System.Collections.Generic;
using System.Security.Claims;

namespace CharacterService
{
	public class CharacterWorkflow : IJwtWorkflow
	{
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }
		private string email = string.Empty;
		private CharacterInformationRepository repo = new CharacterInformationRepository();
		private UdpPeer peer;

		public async Task OnStartAsync(UdpPeer peer)
		{
			this.peer = peer;

			if (await repo.ExistsAsync(email))
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
			CharacterInformation c = await repo.GetAsync(email);
			string token = string.Empty;

			if (sendToken)
			{
				token = JwtTokenHelper.GenerateToken(new List<Claim>
				{
					new Claim(SecurityConfiguration.EmailClaimType, email),
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
				charMessage.Character.Id = email;
				charMessage.Character.Name = charMessage.Character.Name.Trim();
				if (string.IsNullOrEmpty(charMessage.Character.Name))
				{
					SendPlayerCharacter(charMessage.Character);
					return;
				}

				if (await repo.ExistsAsync(email))
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
			email = JwtTokenHelper.GetTokenClaim(token, SecurityConfiguration.EmailClaimType);
		}
	}
}
