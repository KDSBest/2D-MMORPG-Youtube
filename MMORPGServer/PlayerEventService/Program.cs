using Common.Protocol;
using CommonServer.Configuration;
using CommonServer.Udp;
using System;
using System.Threading.Tasks;

namespace EventService
{
    public class Program
	{
		public static async Task Main(string[] args)
		{
            UdpManagerListener udpManagerListener = new UdpManagerListener(ProtocolConstants.ConnectionKey, new PlayerEventUdpListener());

            Console.WriteLine("Open Chat Service");
            await udpManagerListener.StartAsync(PortConfiguration.EventPort);

            await udpManagerListener.UpdateAsync();
            Console.WriteLine("Chat Service is Running...");

            while (udpManagerListener.IsRunning)
            {
                await udpManagerListener.UpdateAsync();
                await Task.Delay(50);
            }

            Console.WriteLine("Udp Manager stopped.");
        }
	}
}
