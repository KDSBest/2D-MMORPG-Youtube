using Common.Extensions;
using Common.Protocol.Chat;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System.Threading.Tasks;

namespace Common.Client.Workflow
{
	public class ChatWorkflow : BaseJwtWorkflow, IWorkflow
	{

		public override async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
		}

		public override async Task OnLatencyUpdateAsync(int latency)
		{
		}

		public override async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			var chatMsg = new ChatMessage();
			if (chatMsg.Read(reader))
			{
				PubSub.Publish(chatMsg);
				return;
			}

			await base.OnReceiveAsync(reader, channel);
		}

		public void SendChatMessage(string message)
		{
			var msg = new ChatMessage
			{
				Message = message
			};
			UdpManager.SendMsg(msg, ChannelType.Reliable);
		}
	}
}
