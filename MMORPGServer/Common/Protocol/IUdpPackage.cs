using ReliableUdp.Utility;

namespace Common.Protocol
{
    public interface IUdpPackage
    {
        bool Read(UdpDataReader reader);
        void Write(UdpDataWriter write);
    }
}
