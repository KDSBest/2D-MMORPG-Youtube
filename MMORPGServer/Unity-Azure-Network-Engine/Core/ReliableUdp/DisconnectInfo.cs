using ReliableUdp.Enums;
using ReliableUdp.Utility;

namespace ReliableUdp
{
	public struct DisconnectInfo
	{
		public DisconnectReason Reason;
		public int SocketErrorCode;
		public UdpDataReader AdditionalData;
	}
}