using Common.Crypto;
using Common.Extensions;
using Common.Protocol.Login;
using Common.Workflow;
using CommonServer;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using CommonServer.Configuration;
using CommonServer.CosmosDb;
using CommonServer.CosmosDb.Model;
using System.Threading.Tasks;
using System.Net.Mail;

namespace LoginService
{
	public class LoginWorkflow : ICryptoWorkflow
	{
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }
		public CryptoProvider Crypto { get; set; }
		private UdpPeer peer;
		private readonly UserInformationRepository repo = new UserInformationRepository();

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
			var registerMessage = new RegisterMessage();

			if (registerMessage.Read(reader))
			{
				string email = Crypto.Decrypt(registerMessage.EMailEnc);
				string password = Crypto.Decrypt(registerMessage.PasswordEnc);
				await RegisterAsync(email, password);
				return;
			}

			var loginMessage = new LoginMessage();
			if (loginMessage.Read(reader))
			{
				string email = Crypto.Decrypt(loginMessage.EMailEnc);
				string password = Crypto.Decrypt(loginMessage.PasswordEnc);
				await LoginAsync(email, password);
				return;
			}
		}

		private async Task RegisterAsync(string email, string password)
		{
			var loginRegisterResponseMessage = new LoginRegisterResponseMessage();

			if (!CheckEmailSyntax(email))
			{
				loginRegisterResponseMessage.Response = LoginRegisterResponse.InvalidEMail;
				UdpManager.SendMsg(peer.ConnectId, loginRegisterResponseMessage, ChannelType.ReliableOrdered);
				return;
			}

			if (!CheckPasswordStrength(password))
			{
				loginRegisterResponseMessage.Response = LoginRegisterResponse.PasswordTooWeak;
				UdpManager.SendMsg(peer.ConnectId, loginRegisterResponseMessage, ChannelType.ReliableOrdered);
				return;
			}

			if (!await CheckEmailIsNew(email))
			{
				loginRegisterResponseMessage.Response = LoginRegisterResponse.AlreadyRegistered;
				UdpManager.SendMsg(peer.ConnectId, loginRegisterResponseMessage, ChannelType.ReliableOrdered);
				return;
			}

			UserInformation user = new UserInformation()
			{
				Id = email,
				PasswordHash = PasswordHash.GetPasswordHash(password)
			};
			await repo.SaveAsync(user, user.Id);

			loginRegisterResponseMessage.Response = LoginRegisterResponse.Successful;
			loginRegisterResponseMessage.Token = JwtTokenHelper.GenerateToken(new List<Claim>
			{
				new Claim(SecurityConfiguration.EmailClaimType, email)
			});

			UdpManager.SendMsg(peer.ConnectId, loginRegisterResponseMessage, ChannelType.ReliableOrdered);
		}

		private bool CheckPasswordStrength(string password)
		{
			return password.Length >= 6;
		}

		private bool CheckEmailSyntax(string email)
		{
			try
			{
				var mail = new MailAddress(email);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private async Task<bool> CheckEmailIsNew(string email)
		{
			return !await repo.ExistsAsync(email);
		}

		private async Task LoginAsync(string email, string password)
		{
			var loginRegisterResponseMessage = new LoginRegisterResponseMessage();

			if (!CheckEmailSyntax(email))
			{
				loginRegisterResponseMessage.Response = LoginRegisterResponse.InvalidEMail;
				UdpManager.SendMsg(peer.ConnectId, loginRegisterResponseMessage, ChannelType.ReliableOrdered);
				return;
			}

			if (await CheckEmailIsNew(email))
			{
				loginRegisterResponseMessage.Response = LoginRegisterResponse.WrongPasswordOrEmail;
				UdpManager.SendMsg(peer.ConnectId, loginRegisterResponseMessage, ChannelType.ReliableOrdered);
				return;
			}

			var user = await repo.GetAsync(email);
			if(!PasswordHash.CheckPassword(user.PasswordHash, password))
			{
				loginRegisterResponseMessage.Response = LoginRegisterResponse.WrongPasswordOrEmail;
				UdpManager.SendMsg(peer.ConnectId, loginRegisterResponseMessage, ChannelType.ReliableOrdered);
				return;
			}

			loginRegisterResponseMessage.Response = LoginRegisterResponse.Successful;
			loginRegisterResponseMessage.Token = JwtTokenHelper.GenerateToken(new List<Claim>
			{
				new Claim(SecurityConfiguration.EmailClaimType, email)
			});

			UdpManager.SendMsg(peer.ConnectId, loginRegisterResponseMessage, ChannelType.ReliableOrdered);
		}
	}
}
