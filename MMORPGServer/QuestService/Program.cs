using Common;
using Common.Protocol;
using CommonServer;
using CommonServer.Configuration;
using CommonServer.CosmosDb;
using CommonServer.Udp;
using System;
using System.Threading.Tasks;

namespace QuestService
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			try
			{
				UdpManagerListener udpManagerListener = new UdpManagerListener(ProtocolConstants.ConnectionKey, new QuestUdpListener());

				Console.WriteLine("Reading all Quest Files...");
				QuestLoader.Load();
				Console.WriteLine($"{QuestLoader.Quests.Count} Quests Loaded.");

				Console.WriteLine("Initialize CosmosDb Connection.");
				var repo = new QuestTrackingRepository();

				Console.WriteLine("Open Quest Service");
				await udpManagerListener.StartAsync(PortConfiguration.QuestPort);

				await udpManagerListener.UpdateAsync();
				Console.WriteLine("Quest Service is Running...");

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
