using Common.Protocol;
using CommonServer.Configuration;
using CommonServer.Udp;
using System;
using System.Threading.Tasks;

namespace InventoryService
{
    public class Program
	{
		public static async Task Main(string[] args)
		{
            UdpManagerListener udpManagerListener = new UdpManagerListener(ProtocolConstants.ConnectionKey, new InventoryUdpListener());

            Console.WriteLine("Open Inventory Service");
            await udpManagerListener.StartAsync(PortConfiguration.InventoryPort);

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
