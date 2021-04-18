using Common.Protocol;
using CommonServer.Udp;
using System;
using System.Threading.Tasks;

namespace LoginService
{
    public class Program
	{
		public static async Task Main(string[] args)
		{
            try
            {
                UdpManagerListener udpManagerListener = new UdpManagerListener(ProtocolConstants.ConnectionKey, new LoginUdpListener());

                Console.WriteLine("Open Login Service");
                await udpManagerListener.Start(3334);

                udpManagerListener.Update();
                Console.WriteLine("Login Service is Running...");

                while (udpManagerListener.IsRunning)
                {
                    udpManagerListener.Update();
                    await Task.Delay(50);
                }

                Console.WriteLine("Udp Manager stopped.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex}");
            }
        }
	}
}
