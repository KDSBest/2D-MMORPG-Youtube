using Common.Extensions;
using Common.Protocol.Combat;
using Common.Workflow;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System.Threading.Tasks;

namespace Common.Client.Workflow
{
	public class CombatWorkflow : BaseJwtWorkflow, IWorkflow
	{

		public override async Task OnDisconnectedAsync(DisconnectInfo disconnectInfo)
		{
		}

		public override async Task OnLatencyUpdateAsync(int latency)
		{
		}

		public override async Task OnReceiveAsync(UdpDataReader reader, ChannelType channel)
		{
			var msg = new DamageDoneMessage();
			if (msg.Read(reader))
			{
				PubSub.Publish(msg);
				return;
			}

			await base.OnReceiveAsync(reader, channel);
		}

		public void SendReqSkillCastMessage(ReqSkillCastMessage skillCastMessage)
		{
			UdpManager.SendMsg(skillCastMessage, ChannelType.Reliable);
		}
	}
}
