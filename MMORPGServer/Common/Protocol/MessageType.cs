using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Protocol
{

    public enum MessageType : byte
    {
        RSAPublicKey = 1,
        AESParameter = 2,
        Chat = 50
    }
}
