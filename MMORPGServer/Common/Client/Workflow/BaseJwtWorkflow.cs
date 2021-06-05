using Common.Client.Interfaces;
using Common.Crypto;
using Common.Extensions;
using Common.IoC;
using Common.Protocol.Crypto;
using Common.PublishSubscribe;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Threading.Tasks;

namespace Common.Client.Workflow
{
	public abstract class BaseJwtWorkflow : IWorkflow
	{
		public CryptoProvider Crypto { get; set; }
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }
		public IPubSub PubSub { get; set; }

		private ITokenProvider tokenProvider;

		public bool HasServerTokenAccepted { get; set; } = false;

		public virtual async Task OnStartAsync(UdpPeer peer)
		{
			PubSub = DI.Instance.Resolve<IPubSub>();
			tokenProvider = DI.Instance.Resolve<ITokenProvider>();
			await SendToken();
		}

		public abstract Task OnDisconnectedAsync(DisconnectInfo disconnectInfo);

		public abstract Task OnLatencyUpdateAsync(int latency);

		public virtual async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			var reqJwt = new ReqJwtMessage();
			if (reqJwt.Read(reader))
			{
				await SendToken();
				return;
			}

			var jwtMessage = new JwtMessage();
			if(jwtMessage.Read(reader))
			{
				if(!string.IsNullOrEmpty(jwtMessage.Token))
				{
					HasServerTokenAccepted = true;
				}
			}
		}

		public async Task SendToken()
		{
			var jwtMsg = new JwtMessage
			{
				Token = tokenProvider.Token
			};
			this.UdpManager.SendMsg(jwtMsg, ChannelType.Reliable);
			Console.WriteLine("Jwt Token send.");
		}

	}
}
