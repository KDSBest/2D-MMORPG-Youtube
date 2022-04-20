using CommonServer.Udp;
using System;
using System.Threading.Tasks;
using CommonServer.Configuration;
using CommonServer.WorldManagement;
using Common.IoC;
using Common.PublishSubscribe;
using Common;
using Common.GameDesign;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using CommonServer.CosmosDb;

namespace MapService
{
    public class Program
    {
		public static async Task Main(string[] args)
        {
            try
			{
				Console.WriteLine("Initialize CosmosDb Connection.");
				var repo = new MapRepository();

				LoadMapData();

				DI.Instance.Register<IPubSub>(() => new PubSub(), RegistrationType.Singleton);
				DI.Instance.Register<IPlayerWorldManagement>(() => new PlayerWorldManagement(), RegistrationType.Singleton);
				DI.Instance.Resolve<IPlayerWorldManagement>().Initialize();
				UdpManagerListener udpManagerListener = new UdpManagerListener(new MapUdpListener());

				Console.WriteLine("Open Map Service");
				await udpManagerListener.StartAsync(PortConfiguration.MapPort);

				await udpManagerListener.UpdateAsync();
				Console.WriteLine("Map Service is Running...");

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

		private static void LoadMapData()
		{
			Console.WriteLine($"Loading Map... {MapConfiguration.MapName}.");
			string mapPath = Path.Combine("Maps", $"{MapConfiguration.MapName}.map");
			string mapFileContent = File.ReadAllText(mapPath);
			var mapData = JsonConvert.DeserializeObject<MapData>(mapFileContent);
			DI.Instance.Register<MapData>(() => mapData, RegistrationType.Singleton);

			Console.WriteLine($"Map has {mapData.NPCs.Count} NPCs with {mapData.NPCs.Count(x => x.IsTeleporter)} teleporter.");
			Console.WriteLine("Map is loaded.");
		}
	}

}
