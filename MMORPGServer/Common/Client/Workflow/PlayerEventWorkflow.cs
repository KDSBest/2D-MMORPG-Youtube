using Common.Extensions;
using Common.Protocol.Chat;
using Common.Protocol.PlayerEvent;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System.Threading.Tasks;

namespace Common.Client.Workflow
{
	public class PlayerEventWorkflow : BaseJwtWorkflow, IWorkflow
	{

		public override async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
		}

		public override async Task OnLatencyUpdateAsync(int latency)
		{
		}

		public override async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			var evMsg = new PlayerEventMessage();
			if (evMsg.Read(reader))
			{
				PubSub.Publish(evMsg);
				return;
			}

			await base.OnReceiveAsync(reader, channel);
		}
	}
}
