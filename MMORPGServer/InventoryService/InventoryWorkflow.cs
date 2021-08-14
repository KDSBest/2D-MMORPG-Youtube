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

namespace InventoryService
{
	public class InventoryWorkflow : IJwtWorkflow
	{
		private InventoryRepository repo = new InventoryRepository();
		public UdpManager UdpManager { get; set; }
		public Func<UdpPeer, IWorkflow, Task> SwitchWorkflowAsync { get; set; }

		private string playerId = string.Empty;
		private UdpPeer peer;

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
			RequestInventoryMessage reqMsg = new RequestInventoryMessage();
			if(reqMsg.Read(reader))
			{
				var inventory = await repo.GetAsync(playerId);
				Inventory cInv = new Inventory();

				if(inventory != null)
					cInv.Items = inventory.Items;
	
				UdpManager.SendMsg(new InventoryMessage()
				{
					Inventory = cInv
				}, ChannelType.ReliableOrdered);
			}
		}

		public void OnToken(string token)
		{
			playerId = JwtTokenHelper.GetTokenClaim(token, SecurityConfiguration.EmailClaimType);
		}
	}
}
