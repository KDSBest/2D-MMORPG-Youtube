using Common.Extensions;
using Common.Protocol.Chat;
using Common.Protocol.Inventory;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System.Threading.Tasks;

namespace Common.Client.Workflow
{
	public class InventoryWorkflow : BaseJwtWorkflow, IWorkflow
	{

		public override async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
		}

		public override async Task OnLatencyUpdateAsync(int latency)
		{
		}

		public override async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			var invMsg = new InventoryMessage();
			if (invMsg.Read(reader))
			{
				PubSub.Publish(invMsg);
				return;
			}

			await base.OnReceiveAsync(reader, channel);
		}

		public void SendRequestInventory()
		{
			UdpManager.SendMsg(new RequestInventoryMessage(), ChannelType.Reliable);
		}
	}
}
