using Common.Protocol;
using CommonServer.Udp;
using System;
using System.Threading.Tasks;

namespace WorldChatService
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                UdpManagerListener udpManagerListener = new UdpManagerListener(ProtocolConstants.ConnectionKey, new ChatUdpListener());

                Console.WriteLine("Open Chat Service");
                await udpManagerListener.Start(3333);

                udpManagerListener.Update();
                Console.WriteLine("Chat Service is Running...");

                int i = 0;
                while (udpManagerListener.IsRunning)
                {
                    udpManagerListener.Update();
                    await Task.Delay(50);
                    i += 50;
                    if (i > 1000)
                    {
                        i = 0;
                        Console.WriteLine("Chat Service still running...");
                    }
                }

                Console.WriteLine("Udp Manager stopped.");
            }
            catch(Exception ex)
			{
                Console.WriteLine($"ERROR: {ex}");
			}
        }
    }
}
