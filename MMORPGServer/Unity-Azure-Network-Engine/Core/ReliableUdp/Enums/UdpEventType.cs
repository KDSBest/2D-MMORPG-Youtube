namespace ReliableUdp.Enums
{
	public enum UdpEventType
	{
		Connect,
		Disconnect,
		Receive,
		ReceiveUnconnected,
		Error,
		ConnectionLatencyUpdated,
		ReceiveAck
	}
}