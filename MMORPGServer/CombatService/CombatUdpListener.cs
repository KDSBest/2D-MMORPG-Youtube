using Common.Udp;
using CommonServer.Workflow;

namespace CombatService
{
	public class CombatUdpListener : BaseUdpListener<JwtWorkflow<CombatWorkflow>>, IUdpListener
    {
        public CombatUdpListener()
		{
        }
    }
}
