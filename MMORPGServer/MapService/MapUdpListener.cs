using Common.Udp;
using CommonServer.Workflow;

namespace MapService
{
	public class MapUdpListener : BaseUdpListener<JwtWorkflow<MapWorkflow>>, IUdpListener
	{
		public MapUdpListener()
		{
		}
	}

}
