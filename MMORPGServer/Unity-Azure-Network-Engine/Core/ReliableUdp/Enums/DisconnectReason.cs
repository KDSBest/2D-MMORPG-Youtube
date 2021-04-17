namespace ReliableUdp.Enums
{
	public enum DisconnectReason
	{
		SocketReceiveError,
		ConnectionFailed,
		Timeout,
		SocketSendError,
		RemoteConnectionClose,
		DisconnectPeerCalled
	}
}