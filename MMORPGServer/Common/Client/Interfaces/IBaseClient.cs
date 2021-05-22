using ReliableUdp;
using System.Threading.Tasks;

namespace Common.Client.Interfaces
{
	public interface IBaseClient
	{
		bool IsConnected { get; }
		UdpPeer Peer { get; set; }

		Task<bool> ConnectAsync(string host = "localhost", int port = 3334, string token = "");
		Task DisconnectAsync();
		Task PollEventsAsync();
	}
}