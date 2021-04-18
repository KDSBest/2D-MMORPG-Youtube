using ReliableUdp;

namespace Common.Udp
{
    public interface IUdpListener : IUdpEventListener
    {
        void Update();
    }
}
