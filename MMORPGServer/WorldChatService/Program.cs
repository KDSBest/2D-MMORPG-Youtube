using Common.Protocol;
using CommonServer.Configuration;
using CommonServer.Udp;
using System;
using System.Threading.Tasks;

namespace WorldChatService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                UdpManagerListener udpManagerListener = new UdpManagerListener(new ChatUdpListener());

                Console.WriteLine("Open Chat Service");
                await udpManagerListener.StartAsync(PortConfiguration.WorldChatPort);

                await udpManagerListener.UpdateAsync();
                Console.WriteLine("Chat Service is Running...");

                while (udpManagerListener.IsRunning)
                {
                    await udpManagerListener.UpdateAsync();
                    await Task.Delay(50);
                }

                Console.WriteLine("Udp Manager stopped.");
            }
            catch(Exception ex)
			{
                Console.WriteLine($"ERROR: {ex}");
                Console.ReadLine();
			}
        }
    }
}
