using System.Threading.Tasks;

namespace Assets.Scripts.ClientWrappers
{
	public interface ICharacterClientWrapper
	{
		bool IsInitialized { get; }

		Task<bool> ConnectAsync(string host, int port);
	}
}