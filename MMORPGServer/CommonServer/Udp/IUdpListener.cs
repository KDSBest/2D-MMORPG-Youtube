using ReliableUdp;

namespace CommonServer.Udp
{
    public interface IUdpListener : IUdpEventListener
    {
        void Update();
    }
}
