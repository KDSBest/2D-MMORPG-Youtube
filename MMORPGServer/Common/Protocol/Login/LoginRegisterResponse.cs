namespace Common.Protocol.Login
{
	public enum LoginRegisterResponse : byte
	{
        Successful = 0,
        Failure = 1,
        AlreadyRegistered = 2
	}

}
