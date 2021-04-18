using System;
using System.Collections.Generic;
using System.Text;

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
        Chat = 50
    }
}
