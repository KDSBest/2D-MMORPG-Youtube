using Common.Udp;
using CommonServer.Workflow;

namespace EventService
{
	public class PlayerEventUdpListener : BaseUdpListener<JwtWorkflow<PlayerEventWorkflow>>, IUdpListener
    {
        public PlayerEventUdpListener()
		{
        }
    }
}
