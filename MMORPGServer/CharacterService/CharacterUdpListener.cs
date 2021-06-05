using Common.Udp;
using CommonServer.Workflow;

namespace CharacterService
{
	public class CharacterUdpListener : BaseUdpListener<JwtWorkflow<CharacterWorkflow>>, IUdpListener
    {
        public CharacterUdpListener()
		{
        }
    }
}
