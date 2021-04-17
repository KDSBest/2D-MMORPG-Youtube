using ReliableUdp.Enums;
using ReliableUdp.Utility;

namespace ReliableUdp
{
	public interface IUdpEventListener
	{
		UdpManager UdpManager { get; set; }

		/// <summary>
		/// New remote peer connected to host, or client connected to remote host
		/// </summary>
		/// <param name="peer">Connected peer object</param>
		void OnPeerConnected(UdpPeer peer);

		/// <summary>
		/// Peer disconnected
		/// </summary>
		/// <param name="peer">disconnected peer</param>
		/// <param name="disconnectInfo">additional info about reason, errorCode or data received with disconnect message</param>
		void OnPeerDisconnected(UdpPeer peer, DisconnectInfo disconnectInfo);

		/// <summary>
		/// Network error (on send or receive)
		/// </summary>
		/// <param name="endPoint">From endPoint (can be null)</param>
		/// <param name="socketErrorCode">Socket error code</param>
		void OnNetworkError(UdpEndPoint endPoint, int socketErrorCode);

		/// <summary>
		/// Received some data
		/// </summary>
		/// <param name="peer">From peer</param>
		/// <param name="reader">DataReader containing all received data</param>
		/// <param name="channel">The channel.</param>
		void OnNetworkReceive(UdpPeer peer, UdpDataReader reader, ChannelType channel);

		/// <summary>
		/// Received ack for local package
		/// </summary>
		/// <param name="peer">From peer</param>
		/// <param name="reader">DataReader containing all received data</param>
		/// <param name="channel">The channel.</param>
		void OnNetworkReceiveAck(UdpPeer peer, UdpDataReader reader, ChannelType channel);

		/// <summary>
		/// Received unconnected message
		/// </summary>
		/// <param name="remoteEndPoint">From address (IP and Port)</param>
		/// <param name="reader">Message data</param>
		/// <param name="messageType">Message type (simple, discovery request or responce)</param>
		void OnNetworkReceiveUnconnected(UdpEndPoint remoteEndPoint, UdpDataReader reader);

		/// <summary>
		/// Latency information updated
		/// </summary>
		/// <param name="peer">Peer with updated latency</param>
		/// <param name="latency">latency value in milliseconds</param>
		void OnNetworkLatencyUpdate(UdpPeer peer, int latency);
    }
}