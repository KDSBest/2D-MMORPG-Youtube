using Common.Protocol.PlayerEvent;
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
using System.Threading;
using CommonServer.CosmosDb;

namespace EventService
{
	public class PlayerEventWorkflow : IJwtWorkflow
	{
		private PlayerEventRepository repo = new PlayerEventRepository();
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }
		private string playerId = string.Empty;
		private Timer timer;
		private UdpPeer peer;

		public async Task OnStartAsync(UdpPeer peer)
		{
			this.peer = peer;
		}

		public async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
			timer.Dispose();
			timer = null;
		}

		public async Task OnLatencyUpdateAsync(int latency)
		{
		}

		public async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
		}

		public void OnToken(string token)
		{
			playerId = JwtTokenHelper.GetTokenClaim(token, SecurityConfiguration.EmailClaimType);
			timer = new Timer(OnTimer, null, GameConfiguration.DefaultBackendTimer, GameConfiguration.DefaultBackendTimer);
		}

		private async void OnTimer(object state)
		{
			if (string.IsNullOrEmpty(playerId) || this.peer == null)
				return;

			var events = repo.GetEvents(playerId);
			foreach(var ev in events)
			{
				UdpManager.SendMsg(new PlayerEventMessage()
				{
					Type = ev.Type,
					CreationDate = ev.CreationDate
				}, ChannelType.ReliableOrdered);
				await repo.RemoveEventAsync(ev);
			}
		}
	}
}
