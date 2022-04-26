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
            UdpManagerListener udpManagerListener = new UdpManagerListener(new PlayerEventUdpListener());

            Console.WriteLine("Open Player Event Service");
            await udpManagerListener.StartAsync(PortConfiguration.EventPort);

            await udpManagerListener.UpdateAsync();
            Console.WriteLine("Player Event Service is Running...");

            while (udpManagerListener.IsRunning)
            {
                await udpManagerListener.UpdateAsync();
                await Task.Delay(50);
            }

            Console.WriteLine("Udp Manager stopped.");
        }
	}
}
