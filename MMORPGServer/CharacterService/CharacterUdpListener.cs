using Common.Extensions;
using Common.Protocol.Chat;
using Common.Udp;
using CommonServer.Redis;
using CommonServer.Workflow;
using ReliableUdp.Enums;
using CommonServer.Configuration;

namespace CharacterService
{
	public class CharacterUdpListener : BaseUdpListener<JwtWorkflow<CharacterWorkflow>>, IUdpListener
    {
        public CharacterUdpListener()
		{
        }
    }
}
