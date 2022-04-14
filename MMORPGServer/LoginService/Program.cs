using Common.Protocol;
using CommonServer.Configuration;
using CommonServer.CosmosDb;
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
                Console.WriteLine("Initialize CosmosDb Connection.");
                var repo = new UserInformationRepository();

                UdpManagerListener udpManagerListener = new UdpManagerListener(new LoginUdpListener());

                Console.WriteLine("Open Login Service");
                await udpManagerListener.StartAsync(PortConfiguration.LoginPort);

                await udpManagerListener.UpdateAsync();
                Console.WriteLine("Login Service is Running...");

                while (udpManagerListener.IsRunning)
                {
                    await udpManagerListener.UpdateAsync();
                    await Task.Delay(50);
                }

                Console.WriteLine("Udp Manager stopped.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex}");
                Console.ReadLine();
            }
        }
	}
}
