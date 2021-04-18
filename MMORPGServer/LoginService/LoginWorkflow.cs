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

namespace LoginService
{
	public class LoginWorkflow : ICryptoWorkflow
	{
		public UdpManager UdpManager { get; set; }
		public Action<UdpPeer, IWorkflow> SwitchWorkflow { get; set; }
		public CryptoProvider Crypto { get; set; }
		private UdpPeer peer;

		public void OnStart(UdpPeer peer)
		{
			this.peer = peer;
		}

		public void OnDisconnected(DisconnectInfo disconnectInfo)
		{
		}

		public void OnLatencyUpdate(int latency)
		{
		}

		public void OnReceive(UdpDataReader reader, ChannelType channel)
		{
			var registerMessage = new RegisterMessage();
			if (registerMessage.Read(reader))
			{
				string email = Crypto.Decrypt(registerMessage.EMailEnc);
				string password = Crypto.Decrypt(registerMessage.PasswordEnc);
				Register(email, password);
			}

			var loginMessage = new LoginMessage();
			if (loginMessage.Read(reader))
			{
				string email = Crypto.Decrypt(loginMessage.EMailEnc);
				string password = Crypto.Decrypt(loginMessage.PasswordEnc);
				Login(email, password);
			}
		}

		private void Register(string email, string password)
		{
			Console.WriteLine($"Register - {email}:{password}");

			var loginRegisterResponseMessage = new LoginRegisterResponseMessage();
			loginRegisterResponseMessage.Response = LoginRegisterResponse.Successful;

			loginRegisterResponseMessage.Token = JwtTokenHelper.GenerateToken(new List<Claim>
			{
				new Claim(SecurityConfiguration.EmailClaimType, email)
			});

			UdpManager.SendMsg(peer.ConnectId, loginRegisterResponseMessage, ChannelType.ReliableOrdered);
		}

		private void Login(string email, string password)
		{
			Console.WriteLine($"Login - {email}:{password}");
			var loginRegisterResponseMessage = new LoginRegisterResponseMessage();
			loginRegisterResponseMessage.Response = LoginRegisterResponse.Successful;

			loginRegisterResponseMessage.Token = JwtTokenHelper.GenerateToken(new List<Claim>
			{
				new Claim(SecurityConfiguration.EmailClaimType, email)
			});

			UdpManager.SendMsg(peer.ConnectId, loginRegisterResponseMessage, ChannelType.ReliableOrdered);
		}
	}
}
