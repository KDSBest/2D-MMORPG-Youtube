using Common.Extensions;
using Common.Protocol.Chat;
using Common.Udp;
using CommonServer.Redis;
using CommonServer.Workflow;
using ReliableUdp.Enums;
using StackExchange.Redis;
using CommonServer.Configuration;

namespace QuestService
{
    public class QuestUdpListener : BaseUdpListener<JwtWorkflow<QuestWorkflow>>, IUdpListener
    {
        public QuestUdpListener()
		{
        }
    }
}
