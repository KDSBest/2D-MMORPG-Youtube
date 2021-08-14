using System.Threading.Tasks;

namespace Assets.Scripts.ClientWrappers
{
	public interface IClientWrapper
	{
		bool IsInitialized { get; }

		Task<bool> ConnectAsync(string host, int port);
	}
}