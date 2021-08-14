using Common.Client.Workflow;
using Common.Workflow;
using ReliableUdp;

namespace Common.Client.Interfaces
{
	public interface IChatClient : IBaseClient<ChatWorkflow>
	{
	}
}