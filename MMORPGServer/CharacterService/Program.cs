using Common.Protocol;
using CommonServer.CosmosDb;
using CommonServer.Udp;
using System;
using System.Threading.Tasks;

namespace CharacterService
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
            try
            {
                Console.WriteLine("Initialize CosmosDb Connection.");
                var repo = new CharacterInformationRepository();

                UdpManagerListener udpManagerListener = new UdpManagerListener(ProtocolConstants.ConnectionKey, new CharacterUdpListener());

                Console.WriteLine("Open Character Service");
                await udpManagerListener.StartAsync(3335);

                await udpManagerListener.UpdateAsync();
                Console.WriteLine("Character Service is Running...");

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
