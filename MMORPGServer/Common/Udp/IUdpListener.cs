using ReliableUdp;
using System.Threading.Tasks;

namespace Common.Udp
{
    public interface IUdpListener : IUdpEventListener
    {
        Task UpdateAsync();
    }
}
