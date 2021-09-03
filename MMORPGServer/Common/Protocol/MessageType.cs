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
        PropState = 81,
        TimeSync = 90,
        RemovePartition = 100,
        PlayerEvent = 110,
        ReqInventory = 120,
        Inventory = 130,
        RemoveState = 200
    }
}
