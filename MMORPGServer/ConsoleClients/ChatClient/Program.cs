using Common.Protocol;
using Common.Protocol.Chat;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using System;
using System.Threading.Tasks;

namespace ChatClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var chatMessage = new ChatMessage();
            var writer = new UdpDataWriter();
            var udp = new UdpManager(new ChatClientListener(), ProtocolConstants.ConnectionKey);
            var peer = udp.Connect("localhost", 3333);

            int delayMs = 50;
            int maxWaitMs = 5000;
            int currentMaxWait = maxWaitMs;
            bool disconnect = false;
            while(peer.ConnectionState == ConnectionState.InProgress && currentMaxWait > 0)
            {
                Console.WriteLine("Connecting...");

                await Task.Delay(delayMs);

                currentMaxWait -= delayMs;
                udp.PollEvents();
            }

            var updateThread = Task.Run(async () =>
            {
                while (peer.ConnectionState == ConnectionState.Connected && !disconnect)
                {
                    await Task.Delay(delayMs);
                    udp.PollEvents();
                }
            });

            while (peer.ConnectionState == ConnectionState.Connected && !disconnect)
            {
                chatMessage.Message = Console.ReadLine();
                if(string.IsNullOrEmpty(chatMessage.Message))
                {
                    disconnect = true;
                    continue;
                }

                writer.Reset();
                chatMessage.Write(writer);

                peer.Send(writer, ChannelType.ReliableOrdered);
            }

            updateThread.Wait(maxWaitMs);

            Console.WriteLine("Disconnected.");
        }
    }
}
