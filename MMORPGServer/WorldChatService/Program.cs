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
            UdpManagerListener udpManagerListener = new UdpManagerListener(ProtocolConstants.ConnectionKey, new ChatUdpListener());

            Console.WriteLine("Open Chat Service");
            await udpManagerListener.Start(3333);

            udpManagerListener.Update();
            Console.WriteLine("Chat Service is Running...");

            while (udpManagerListener.IsRunning)
            {
                udpManagerListener.Update();
                await Task.Delay(50);
            }
        }
    }
}
