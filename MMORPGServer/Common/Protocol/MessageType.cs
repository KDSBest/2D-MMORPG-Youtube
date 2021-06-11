namespace Common.Protocol
{

	public enum MessageType : byte
    {
        RSAPublicKey = 1,
        AESParameter = 2,
        Login = 10,
        Register = 11,
        LoginRegisterResponse = 12,
        Jwt = 13,
        ReqJwt = 14,
        Chat = 50,
        Character = 60,
        CharacterRequest = 61,
        PlayerState = 80,
        TimeSync = 81,
        RemoveState = 82,
        RemovePartition = 83
    }
}
