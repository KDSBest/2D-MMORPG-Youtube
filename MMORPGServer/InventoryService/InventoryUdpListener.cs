using Common.Udp;
using CommonServer.Workflow;

namespace InventoryService
{
	public class InventoryUdpListener : BaseUdpListener<JwtWorkflow<InventoryWorkflow>>, IUdpListener
    {
        public InventoryUdpListener()
		{
        }
    }
}
