using Common.Udp;
using CommonServer.Workflow;

namespace LoginService
{
	public class LoginUdpListener : BaseUdpListener<LoginWorkflow>, IUdpListener
    {
    }
}
