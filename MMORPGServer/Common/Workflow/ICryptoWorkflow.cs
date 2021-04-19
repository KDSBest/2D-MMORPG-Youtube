using Common.Crypto;

namespace Common.Workflow
{
	public interface ICryptoWorkflow : IWorkflow
	{
		CryptoProvider Crypto { get; set; }
	}
}
