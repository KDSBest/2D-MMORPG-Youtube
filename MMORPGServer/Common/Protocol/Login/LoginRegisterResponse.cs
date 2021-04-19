namespace Common.Protocol.Login
{
	public enum LoginRegisterResponse : byte
	{
        Successful = 0,
        WrongPasswordOrEmail = 1,
        AlreadyRegistered = 2,
		InvalidEMail = 3,
		PasswordTooWeak = 4
	}

}
