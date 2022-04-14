using Common.Protocol;
using CommonServer.Configuration;
using CommonServer.Udp;
using System;
using System.Threading.Tasks;

namespace CombatService
{
    public class Program
	{
		public static async Task Main(string[] args)
		{
            UdpManagerListener udpManagerListener = new UdpManagerListener(new CombatUdpListener());

            Console.WriteLine("Open Combat Service");
            await udpManagerListener.StartAsync(PortConfiguration.CombatPort);

            await udpManagerListener.UpdateAsync();
            Console.WriteLine("Combat Service is Running...");

            while (udpManagerListener.IsRunning)
            {
                await udpManagerListener.UpdateAsync();
                await Task.Delay(50);
            }

            Console.WriteLine("Udp Manager stopped.");
        }
	}
}
