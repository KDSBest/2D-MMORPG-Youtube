using Common.Workflow;

namespace CommonServer.Workflow
{
	public interface IJwtWorkflow : IWorkflow
	{
		void OnToken(string token);
	}
}
