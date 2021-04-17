namespace ReliableUdp.Enums
{
	public enum PacketType : byte
	{
		Unreliable = 0,
		Reliable = 1,
		UnreliableOrdered = 2,
		ReliableOrdered = 3,
		AckReliable = 4,
		AckReliableOrdered = 5,
		Ping = 6,
		Pong = 7,
		ConnectRequest = 8,
		ConnectAccept = 9,
		Disconnect = 10,
		UnconnectedMessage = 11,
		MtuCheck = 12,
		MtuOk = 13,
		Merged = 14,

        // NOTE: if you add types keep this the last one in value
        PacketTypeTooHigh = 15
	}
}